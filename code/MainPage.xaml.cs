#define iOS
// #define UWP

//#define MemoBoardPan

//#define BoxDisplay    // Set some BoxViews for Debug

using System;
using Xamarin.Forms;

namespace DragOnScrollView
{
    public partial class MainPage : ContentPage
    {
        ContentView TappedObject, CreatedArrow;   // Lastly Tapped ContentView
        Board memoBoard = new Board();

#if BoxDisplay
        // BoxViews to check some positions of the parts
        BoxView box1 = new BoxView()
        {
            TranslationX = 0,
            TranslationY = 0,
            WidthRequest = 20,
            HeightRequest = 20,
            BackgroundColor = Color.Red
        };
        BoxView box2 = new BoxView()
        {
            TranslationX = 0,
            TranslationY = 0,
            WidthRequest = 15,
            HeightRequest = 15,
            BackgroundColor = Color.Blue
        };
        BoxView box3 = new BoxView()
        {
            TranslationX = 0,
            TranslationY = 0,
            WidthRequest = 10,
            HeightRequest = 10,
            BackgroundColor = Color.Green
        };
#endif

        public MainPage()
        {
            InitializeComponent();

#if MemoBoardPan
            // Pan Gesture Event on MemoBoard
            var panGesture = new PanGestureRecognizer();
            panGesture.PanUpdated += OnPanMemoBoard;
            MemoBoard.GestureRecognizers.Add(panGesture);
#endif
        }


        // Get "ScrollBoard" size properly and set some variables
        void GetScrollBoardSize(object sender, EventArgs args)
        {
            memoBoard.OriginalW = ScrollBoard.Width;
            memoBoard.OriginalH = ScrollBoard.Height;

            Label1.Text = string.Format("ScrollBoard Size = ( {0}, {1} )",
                                        ScrollBoard.Width, ScrollBoard.Height);

#if BoxDisplay
            MemoBoard.Children.Add(box1);
            MemoBoard.Children.Add(box2);
            MemoBoard.Children.Add(box3);
#endif
            StatusLabel.Text = "Get ScrollBoard Size";
        }


        // Create a new Memo and stick it onto "ScrollBoard"
        void StickMemo(object sender, EventArgs args)
        {
            Memo NewMemo = new Memo();

            // Set the initial position and offsets
#if MemoBoardPan
            NewMemo.Body.TranslationX = memoBoard.Display.X;
            NewMemo.Body.TranslationY = memoBoard.Display.Y;
            NewMemo.Offset = new Point()
            {
                X = memoBoard.Display.X,
                Y = memoBoard.Display.Y
            };
#else
            NewMemo.Body.TranslationX = ScrollBoard.ScrollX;
            NewMemo.Body.TranslationY = ScrollBoard.ScrollY;
            NewMemo.Offset = new Point()
            {
                X = ScrollBoard.ScrollX,
                Y = ScrollBoard.ScrollY
            };
#endif

            // Add Tap, Pan, Pinch Gesture Event
            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += OnTapMemo;
            NewMemo.Body.GestureRecognizers.Add(tapGesture);

            var panGesture = new PanGestureRecognizer();
            panGesture.PanUpdated += OnPanMemo;
            NewMemo.Body.GestureRecognizers.Add(panGesture);

            var pinchGesture = new PinchGestureRecognizer();
            pinchGesture.PinchUpdated += OnPinchMemo;
            NewMemo.Body.GestureRecognizers.Add(pinchGesture);


            // Add Editor Event
            Editor editor = (Editor)NewMemo.Body.Content;
            editor.TextChanged  += EditorTextChanged;
            editor.Focused      += EditorFocused;
            editor.Unfocused    += EditorUnfocused;

            // Add this new Memo to the Memos collection
            //               and "MemoBoard" in "ScrollBoard"
            Memos.Add(NewMemo);
            MemoBoard.Children.Add(NewMemo.Body);

            StatusLabel.Text = "Memo Sticked";
        }


        // Create an new Arrow and Link it to the Memo "From"
        void LinkMemo(object sender, EventArgs args)
        {
            var button = (Button)sender;

            // If pressed Cacel Button, 
            //       Cancel the Arrow Linking to Memo "To"
            if (button.Text == "Cancel")
            {
                MemoBoard.Children.Remove(CreatedArrow);
                StatusLabel.Text = "Cancel Linking";
                button.Text = "->"; // toggle button.Text
                return;             // Exit "LinkMemo" method
            }

            // Create a new Arrow
            Arrow NewArrow = new Arrow();

            // Set Two Memos linked by this Arrow
            NewArrow.From = (Memo)WhichObject(TappedObject);
            NewArrow.To = (Memo)WhichObject(TappedObject);

            // Set the initial position
            NewArrow.Body.TranslationX
                = TappedObject.TranslationX + TappedObject.WidthRequest
                + NewArrow.Body.HeightRequest / 2 - NewArrow.Body.WidthRequest / 2;
            NewArrow.Body.TranslationY
                = TappedObject.TranslationY + TappedObject.HeightRequest / 2
                - NewArrow.Body.HeightRequest / 2;

            // Add Arrow Parts on the Background (ContentView) of the Arrow
            var layout = (AbsoluteLayout)NewArrow.Body.Content;
            layout.Children.Add(NewArrow.Line);
            layout.Children.Add(NewArrow.Title);
            layout.Children.Add(NewArrow.TitleInput);
            layout.Children.Add(NewArrow.ArrowHead);

            // Add Tap Gesture Event
            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += OnTapMemo;
            NewArrow.Body.GestureRecognizers.Add(tapGesture);

            // Add Entry Input Completed Event
            NewArrow.TitleInput.Completed += TitleInputCompleted;

            // Add this new Arrow to the Arrows collection of 
            //      the Memo "From" and "absoluteLayout" in "ScrollBoard"
            NewArrow.From.Arrows.Add(NewArrow);
            MemoBoard.Children.Add(NewArrow.Body);

            //TappedObject = NewArrow.Body;
            CreatedArrow = NewArrow.Body;

            StatusLabel.Text = "Linking Memo";
            button.Text = "Cancel";     // toggle button.Text
        }


        // Delete Object (Memo or Arrow)
        void DeleteObject(object sender, EventArgs args)
        {
            switch (StatusLabel.Text)
            {
                case "Memo Tapped":
                    Memo TappedMemo = (Memo)WhichObject(TappedObject);

                    foreach (Arrow arrow in TappedMemo.Arrows)
                    {
                        // Remove all arrows from Memo and MemoBoard
                        if (arrow.From == TappedMemo) arrow.To.Arrows.Remove(arrow);
                        if (arrow.To == TappedMemo) arrow.From.Arrows.Remove(arrow);
                        MemoBoard.Children.Remove(arrow.Body);
                    }

                    Memos.Remove(TappedMemo);
                    StatusLabel.Text = "Memo Deleted";
                    break;

                case "Arrow Tapped":
                case "Linking Memo":
                    Arrow TappedArrow = (Arrow)WhichObject(TappedObject);

                    Memo from = TappedArrow.From; from.Arrows.Remove(TappedArrow);
                    Memo to = TappedArrow.To; to.Arrows.Remove(TappedArrow);

                    LinkMemoButton.Text = "->"; // change the button label
                    StatusLabel.Text = "Arrow Deleted";
                    break;
            }

            MemoBoard.Children.Remove(TappedObject);
            DeleteObjectButton.IsEnabled = false;
        }


        // Panning Memo, and Update the Memo and the Arrows linked it
        void OnPanMemo(object sender, PanUpdatedEventArgs e)
        {
            ContentView PannedObject = (ContentView)sender;
            Memo PannedMemo = (Memo)WhichObject(PannedObject);

            switch (e.StatusType)
            {
                case GestureStatus.Started:
                    PannedObject.BackgroundColor = Color.Black;
                    StatusLabel.Text = "Panning Memo";
                    break;

                case GestureStatus.Running:
                    PannedObject.TranslationX
                        = PannedMemo.Offset.X + e.TotalX * PannedObject.Scale;
                    PannedObject.TranslationY
                        = PannedMemo.Offset.Y + e.TotalY * PannedObject.Scale;

                    foreach (Arrow arrow in PannedMemo.Arrows)
                        UpdateArrow(arrow);

                    Label1.Text = string.Format("Memo Position (X, Y) = ( {0:0.0} , {1:0.0} )",
                                                PannedObject.TranslationX, PannedObject.TranslationY);
                    Label2.Text = string.Format("Memo Moved (dX, dY) = ( {0:0.0} , {1:0.0} )",
                                                e.TotalX, e.TotalY);
                    break;

                case GestureStatus.Completed:
                    PannedObject.BackgroundColor = Color.Gray;

                    // Store the translation applied during the pan
                    PannedMemo.Offset.X = PannedObject.TranslationX;
                    PannedMemo.Offset.Y = PannedObject.TranslationY;

#if !MemoBoardPan
                    // If thie object is moved out of the ScrollBoard, expand the ScrollBoard
                    if (PannedMemo.Offset.X + PannedObject.Width > MemoBoard.Width) // expand to Right
                        MemoBoard.WidthRequest = MemoBoard.Width +
                            PannedMemo.Offset.X + PannedObject.Width - MemoBoard.Width + 10;
                    if (PannedMemo.Offset.Y + PannedObject.Height > MemoBoard.Height)   // expand to 
                        MemoBoard.HeightRequest = MemoBoard.Height +
                            PannedMemo.Offset.Y + PannedObject.Height - MemoBoard.Height + 10;
                    if (PannedMemo.Offset.X < 0)
                    {
                        double dx = (-PannedMemo.Offset.X) + 10;
                        MemoBoard.WidthRequest = MemoBoard.Width + dx;
                        ScrollBoard.ScrollToAsync(dx, ScrollBoard.ScrollY, false);

                        // Shift all Memos and Arrows
                        foreach (Memo memo in Memos)
                        {
                            memo.Body.TranslationX += dx;
                            memo.Offset.X = memo.Body.TranslationX;
                            foreach (Arrow arrow in memo.Arrows)
                            {
                                arrow.Body.TranslationX += dx;
                                UpdateArrow(arrow);
                            }
                        }
                    }
                    if (PannedMemo.Offset.Y < 0)
                    {
                        double dy = (-PannedMemo.Offset.Y) + 10;
                        MemoBoard.HeightRequest = MemoBoard.Height + dy;
                        ScrollBoard.ScrollToAsync(ScrollBoard.ScrollX, dy, false);

                        // Shift all Memos and Arrows
                        foreach (Memo memo in Memos)
                        {
                            memo.Body.TranslationY += dy;
                            memo.Offset.Y = memo.Body.TranslationY;
                            foreach (Arrow arrow in memo.Arrows)
                            {
                                arrow.Body.TranslationY += dy;
                                UpdateArrow(arrow);
                            }
                        }
                    }
#endif
                    StatusLabel.Text = "Pan Completed";
                    break;

                    // If this Memo is removed, Cancel event is raised
                    //case GestureStatus.Canceled:
                    //    DisplayAlert("", "Canceled", "Ok");
                    //    break;
            }
        }


        // Pinching Memo and Update the Memo and the Arrows linked it
        void OnPinchMemo(object sender, PinchGestureUpdatedEventArgs e)
        {
            ContentView PinchedObject = (ContentView)sender;
            Memo PinchedMemo = (Memo)WhichObject(PinchedObject);

            switch (e.Status)
            {
                case GestureStatus.Started:
                    PinchedMemo.ScaleOrigin = e.ScaleOrigin;
                    StatusLabel.Text = "Pinching Memo";
                    break;

                case GestureStatus.Running:
                    double dx = e.ScaleOrigin.X - PinchedMemo.ScaleOrigin.X;

                    // Horizontal Pinch by 2-finger Drag
                    PinchedObject.WidthRequest *= (1 + dx);

                    // Vertical Pinch by 2-finger Pinch
                    PinchedObject.HeightRequest *= (1.0 + (e.Scale - 1.0) * 0.5);

                    // Store ScaleOrigin
                    PinchedMemo.ScaleOrigin = e.ScaleOrigin;

                    Label1.Text = string.Format("ScaleOrigin = ( {0:0.00} , {1:0.00} )",
                                                e.ScaleOrigin.X, e.ScaleOrigin.Y);
                    Label2.Text = string.Format("Scale (X, Y) = ({0:0.00}, {1:0.00}", dx, e.Scale);
                    break;

                case GestureStatus.Completed:
                    // Store the translation applied during the pinch
                    PinchedMemo.Offset.X = PinchedObject.TranslationX;
                    PinchedMemo.Offset.Y = PinchedObject.TranslationY;

                    StatusLabel.Text = "Pinch Completed";
                    break;
            }

        }


        // Tap Object (Memo / Arrow)
        void OnTapMemo(object sender, EventArgs args)
        {
            // Reset all Memos
            foreach (Memo memo in Memos)
            {
                memo.Body.BackgroundColor = Color.Gray;
                memo.Body.Content.InputTransparent = true;
            }

            TappedObject = (ContentView)sender;
            object o = WhichObject(TappedObject);
            Arrow arrow = new Arrow();

            switch (o.GetType().Name)
            {
                case "Memo":    // Tapped object is Memo
                    MemoBoard.Children.Remove(TappedObject);
                    MemoBoard.Children.Add(TappedObject);

                    TappedObject.BackgroundColor = Color.Black;
                    TappedObject.Content.InputTransparent = false;

                    LinkMemoButton.IsEnabled = true;
                    DeleteObjectButton.IsEnabled = true;

                    StatusLabel.Text = "Memo Tapped";

                    // A new arrow has been already linked "From" Memo, 
                    //       so the Tapped Memo will be its "To" Memo
                    if (LinkMemoButton.Text == "Cancel")
                    {
                        arrow = (Arrow)WhichObject(CreatedArrow);
                        arrow.To = (Memo)WhichObject(TappedObject);
                        arrow.To.Arrows.Add(arrow);
                        UpdateArrow(arrow);
                        LinkMemoButton.Text = "->";
                        StatusLabel.Text = "Memos Linked";
                    }
                    break;

                case "Arrow":   // Tapped object is Arrow                    
                    arrow = (Arrow)WhichObject(TappedObject);

                    // Enable Title Input
                    arrow.TitleInput.IsVisible = true;
                    arrow.TitleInput.IsEnabled = true;
                    arrow.TitleInput.Focus();

                    // Disable LinkMemoButton
                    LinkMemoButton.IsEnabled = false;

                    StatusLabel.Text = "Arrow Tapped";
                    break;
            }
        }


        // Update all Arrow parts' altitude
        void UpdateArrow(Arrow arrow)
        {
            double length = arrow.Length;

            arrow.Body.HeightRequest = length;
            arrow.Body.TranslationX
                = (arrow.A.X + arrow.B.X - arrow.Body.WidthRequest) / 2;
            arrow.Body.TranslationY
                = (arrow.A.Y + arrow.B.Y - arrow.Body.HeightRequest) / 2;
            arrow.Body.Rotation = arrow.Theta;

            arrow.Line.HeightRequest = length;

            arrow.Title.WidthRequest = length;
            arrow.Title.TranslationX = -length / 2;
            arrow.Title.TranslationY = length / 2;

            arrow.TitleInput.WidthRequest = length;
            arrow.TitleInput.TranslationX = -length / 2;
            arrow.TitleInput.TranslationY = length / 2;
        }


        // If Return Key is detected in the editor, expand its vertical size
        void EditorTextChanged(object sender, TextChangedEventArgs e)
        {
            Editor editor = (Editor)sender;
            Memo memo = (Memo)WhichObject(editor);

#if iOS
            if (e.NewTextValue.EndsWith("\n"))  // for iOS
#elif UWP
            if (e.NewTextValue.EndsWith("\r"))  // for UWP
#endif
                memo.Body.HeightRequest += 20;
            
            StatusLabel.Text = "Editting";
        }

        // Change the border color, when the editor is focused
        void EditorFocused(object sender, EventArgs args)
        {
            Editor editor = (Editor)sender;

            Memo memo = (Memo)WhichObject(editor);
            memo.Body.BackgroundColor = Color.Maroon;

            StatusLabel.Text = "Editor Focused";
        }

        // Reset the border color, when the editor is unfocused
        void EditorUnfocused(object sender, EventArgs args)
        {
            Editor editor = (Editor)sender;

            Memo memo = (Memo)WhichObject(editor);
            memo.Body.BackgroundColor = Color.Gray;
            memo.Body.Content.InputTransparent = true;
            //memo.Body.Content.Unfocus();

            StatusLabel.Text = "Editor Unfocused";
        }

        // When Title Input is completed, change the Title and make the Entry invisible
        void TitleInputCompleted(object sender, EventArgs args)
        {
            Entry entry = (Entry)sender;
            Arrow arrow = (Arrow)WhichObject(TappedObject);

            arrow.Title.Text = entry.Text;
            entry.IsVisible = false;

            StatusLabel.Text = "Link Title Input Completed";
        }


        // Raised when "ScrollBoard" is scrolled
        void IsScrolled(object sender, EventArgs e)
        {
            Label3.Text = string.Format("Scroll (X, Y) = ({0}, {1})",
                                        ScrollBoard.ScrollX, ScrollBoard.ScrollY);
            StatusLabel.Text = "Scrolling";
        }

        // Scale change by the Slider
        void ScaleChanged(object sender, ValueChangedEventArgs args)
        {
            double value = args.NewValue;
            Label3.Text = "ScaleValue = " + value.ToString();
            MemoBoard.Scale = value;

            StatusLabel.Text = "Scale Changing";
        }


#if MemoBoardPan
        // Panning MemoBoard
        void OnPanMemoBoard(object sender, PanUpdatedEventArgs e)
        {
            AbsoluteLayout PannedObject = (AbsoluteLayout)sender;

            switch (e.StatusType)
            {
                case GestureStatus.Started:
                    StatusLabel.Text = "Panning MemoBoard";
                    break;

                case GestureStatus.Running:
                    PannedObject.TranslationX
                                = memoBoard.Offset.X + e.TotalX * PannedObject.Scale;
                    PannedObject.TranslationY
                                = memoBoard.Offset.Y + e.TotalY * PannedObject.Scale;
                    break;

                case GestureStatus.Completed:
                    // Store the translation applied during the pan
                    memoBoard.Offset.X = PannedObject.TranslationX;
                    memoBoard.Offset.Y = PannedObject.TranslationY;

                    // Update Display Position
                    memoBoard.Display.X = memoBoard.OffsetMax.X - PannedObject.TranslationX;
                    memoBoard.Display.Y = memoBoard.OffsetMax.Y - PannedObject.TranslationY;

                    // If dx1, dy1 > 0, memoBoard should be expanded to Left and Up
                    double dx1 = PannedObject.TranslationX - memoBoard.OffsetMax.X;
                    double dy1 = PannedObject.TranslationY - memoBoard.OffsetMax.Y;

                    // If dx2, dy2 > 0, memoBoard should be expanded to Right and Down
                    double dx2 = memoBoard.Display.X + memoBoard.OriginalW
                                          - PannedObject.WidthRequest;
                    double dy2 = memoBoard.Display.Y + memoBoard.OriginalH
                                          - PannedObject.HeightRequest;
                    //- (PannedObject.TranslationY + PannedObject.Height);


                    Label2.Text = PannedObject.TranslationY.ToString();
                    //Label3.Text = string.Format("(dx, dy) = ( {0:0.00} , {1:0.00} )", dx, dy);
                    //Label2.Text = string.Format("e.Totoal X, Y = ( {0:0.00} , {1:0.00} )",
                    //                            e.TotalX, e.TotalY);


                    // Expand MemoBoard
                    if (dx1 > 0)  // expand to Left
                    {
                        PannedObject.WidthRequest = PannedObject.Width + dx1;
                        memoBoard.Display.X = 0;

#if BoxDisplay
                        // Green BoxView
                        box3.TranslationX = PannedObject.WidthRequest - 10;
#endif
                    }
                    else  // dx1 <= 0
                    {
#if BoxDisplay
                        // Green BoxView
                        box3.TranslationX = PannedObject.Width - 10;
#endif
                        if (dx2 > 0)  // expand to Right
                        {
                            PannedObject.WidthRequest =
                                memoBoard.Display.X + memoBoard.OriginalW;
                        }
                    }

                    if (dy1 > 0)  // expand to Up
                    {
                        PannedObject.HeightRequest = PannedObject.Height + dy1;
                        memoBoard.Display.Y = 0;

#if BoxDisplay
                        // Green BoxView
                        box3.TranslationY = PannedObject.HeightRequest - 10;
#endif
                    }
                    else  // dy1 <= 0
                    {
#if BoxDisplay
                        // Green BoxView
                        box3.TranslationY = PannedObject.Height - 10;
#endif

                        //if (PannedObject.TranslationY + PannedObject.Height
                        //< memoBoard.Display.Y + memoBoard.OriginalH)
                        if (dy2 > 0)  // expand to Down
                        {
                            PannedObject.HeightRequest =
                                memoBoard.Display.Y + memoBoard.OriginalH;
                        }
                    }


#if BoxDisplay
                    // Red BoxView
                    box1.TranslationX = memoBoard.Display.X;
                    box1.TranslationY = memoBoard.Display.Y;
                    // Blue BoxView
                    box2.TranslationX = memoBoard.Display.X + memoBoard.OriginalW - 15;
                    box2.TranslationY = memoBoard.Display.Y + memoBoard.OriginalH - 15;
#endif


                    //Label2.Text = string.Format("MemoBoard Size = ( {0:0.0} , {1:0.0} )",
                    //                    PannedObject.Width, PannedObject.Height);
                    Label3.Text = string.Format("Display X, Y = ( {0:0.0} , {1:0.0} )",
                                                memoBoard.Display.X, memoBoard.Display.Y);

                    // Shift MemoBoard to Left and Up
                    if (dx1 > 0)
                    {
                        memoBoard.OffsetMax.X = PannedObject.TranslationX;
                        ScrollBoard.ScrollToAsync(PannedObject.TranslationX,
                                                      ScrollBoard.ScrollY, false);
                    }
                    if (dy1 > 0)
                    {
                        memoBoard.OffsetMax.Y = PannedObject.TranslationY;
                        ScrollBoard.ScrollToAsync(ScrollBoard.ScrollX,
                                                      PannedObject.TranslationY, false);
                    }


                    //Label2.Text = string.Format("OffsetMax = ( {0:0.0} , {1:0.0} )",
                    //                            memoBoard.OffsetMax.X, memoBoard.OffsetMax.Y);


                    // Shift all Memos and Arrows
                    foreach (Memo memo in Memos)
                    {
                        if (dx1 > 0) memo.Body.TranslationX += dx1;
                        if (dy1 > 0) memo.Body.TranslationY += dy1;
                        memo.Offset.X = memo.Body.TranslationX;
                        memo.Offset.Y = memo.Body.TranslationY;

                        foreach (Arrow arrow in memo.Arrows)
                        {
                            if (dx1 > 0) arrow.Body.TranslationX += dx1;
                            if (dy1 > 0) arrow.Body.TranslationY += dy1;
                            UpdateArrow(arrow);
                        }
                    }

                    StatusLabel.Text = "";
                    break;
            }

            Label1.Text = string.Format("(X, Y) = ( {0:0.00} , {1:0.00} )",
                                        PannedObject.TranslationX, PannedObject.TranslationY);
            //Label2.Text = string.Format("e.Total = ( {0:0.00} , {1:0.00} )",
            //                            e.TotalX, e.TotalY);

        }
#endif


        // Pinching MemoBoard
        void OnPinchMemoBoard(object sender, PinchGestureUpdatedEventArgs e)
        {
            AbsoluteLayout PinchedObject = (AbsoluteLayout)sender;

            switch (e.Status)
            {
                case GestureStatus.Started:
                    StatusLabel.Text = "Pinching MemoBoard";
                    break;

                case GestureStatus.Running:
                    PinchedObject.Scale *= e.Scale;

                    Label1.Text = string.Format("ScaleOrigin = ( {0:0.00} , {1:0.00} )",
                                                e.ScaleOrigin.X, e.ScaleOrigin.Y);
                    Label2.Text = string.Format("e.Scale = {0:0.00},  e.Scale Total = {1:0.00}",
                                                e.Scale, PinchedObject.Scale);

                    break;

                case GestureStatus.Completed:
#if BoxDisplay
                    // Red BoxView
                    box1.TranslationX = PinchedObject.TranslationX;
                    box1.TranslationY = PinchedObject.TranslationY;
                    // Blue BoxView
                    box2.TranslationX = PinchedObject.TranslationX + PinchedObject.Width;
                    box2.TranslationY = PinchedObject.TranslationY + PinchedObject.Height;
#endif

                    StatusLabel.Text = "MemoBoard Pinch Completed";
                    break;
            }
        }


        // If tapped MemoBoard, reset MemoBoard Scale
        void OnTapMemoBoard(object sender, EventArgs e)
        {
            AbsoluteLayout tappedObject = (AbsoluteLayout)sender;

            tappedObject.Scale = 1.0;
            ScaleSlider.Value = 1.0;

            StatusLabel.Text = "MemoBoard Tapped";
        }


        // Identify whether the object is ContentView or Editor
        object WhichObject(object o)
        {
            switch(o.GetType().Name)
            {
                case "ContentView":
                    foreach (Memo memo in Memos){                        
                        if (memo.Body == (ContentView)o) return memo;
                        foreach (Arrow arrow in memo.Arrows)
                            if (arrow.Body == (ContentView)o) return arrow;
                    }
                    break;

                case "Editor":
                    foreach (Memo memo in Memos)
                        if (memo.Body.Content == (Editor)o) return memo;
                    break;
            }

            StatusLabel.Text = "Error in WhichObject";
            return null;
        }
    }
}

