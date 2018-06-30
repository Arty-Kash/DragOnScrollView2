using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace DragOnScrollView
{
    public partial class MainPage : ContentPage
    {
        class Memo
        {
            // Main ContentView surrounding an Editor
            private ContentView body = new ContentView()
            {                
                WidthRequest = 120,
                HeightRequest = 30,
                BackgroundColor = Color.Gray,
                Padding = new Thickness(1, 1, 1, 1),
                Content = new Editor
                {
                    FontSize = 14,
                    InputTransparent = true,
                    Keyboard = Keyboard.Plain
                }
            };
            public ContentView Body { get { return this.body; } }
            
            public Point Offset;   // Position when Tap/Pan/Pinch gesture starts

            public double X { get { return Body.TranslationX; } }
            public double Y { get { return Body.TranslationY; } }
            public double W { get { return Body.WidthRequest; } }
            public double H { get { return Body.HeightRequest; } }

            // Arrows linked this Memo
            public List<Arrow> Arrows = new List<Arrow>();

            // Pinch Gesture Scale Origin
            public Point ScaleOrigin;
        }
        List<Memo> Memos = new List<Memo>();


        // Arrow from Memo "From" to Memo "To"
        class Arrow //: INotifyPropertyChanged
        {
            // This Arrow links the Memo "From" and "To"
            public Memo From, To;            

            // Contents of the Arrow : Body, Line, Title, TitleInput
            // Body : Background to detect Tap Gesture
            private static double ArrowWidth = 20;
            private static double ArrowHeight = 80;
            private ContentView body = new ContentView()
            {
                WidthRequest = ArrowWidth,
                HeightRequest = ArrowHeight,
                Rotation = 90,
                //BackgroundColor = Color.Yellow,
                Content = new AbsoluteLayout
                {
                    BackgroundColor = Color.Transparent,
                    InputTransparent = true
                }
            };
            public ContentView Body { get { return this.body; } }


            // Main Body of the Arrow
            private BoxView line = new BoxView()
            {
                WidthRequest = 2,
                HeightRequest = ArrowHeight,
                TranslationX = ArrowWidth / 2 - 1,
                //TranslationY = NewArrow.Body.HeightRequest / 2,
                BackgroundColor = Color.Black,
                InputTransparent = true
            };
            public BoxView Line { get { return this.line; } }

            // Create Arrow Head
            private Label arrowHead = new Label()
            {
                Text = "▲",
                FontSize = 15,
                WidthRequest = ArrowWidth,
                HeightRequest = ArrowWidth,
                HorizontalTextAlignment = TextAlignment.Center,
                TranslationY = - ArrowWidth / 2
            };
            public Label ArrowHead { get { return this.arrowHead; } }


            // Title Label
            private Label title = new Label()
            {
                Text = "Title",
                WidthRequest = ArrowHeight,
                HeightRequest = 20,
                //BackgroundColor = Color.Blue,
                TranslationX = - ArrowHeight / 2,
                TranslationY = ArrowHeight / 2 - 20 / 2,
                HorizontalTextAlignment = TextAlignment.Center,
                Rotation = -90
            };
            public Label Title { get { return this.title; } }

            // Entry to change the title
            private Entry titleInput = new Entry()
            {
                HorizontalTextAlignment = TextAlignment.Center,
                WidthRequest = ArrowHeight,
                HeightRequest = 20,
                //BackgroundColor = Color.Red,
                TranslationX = - ArrowHeight / 2,
                TranslationY = ArrowHeight / 2 - 20 / 2,
                Rotation = -90,
                IsVisible = false,
                Keyboard = Keyboard.Plain
            };
            public Entry TitleInput { get { return this.titleInput; } }


            // Calculate Positions of A(Start) and B(End) points of the Arrow 
            List<Point> Points()
            {
                List<Point> points = new List<Point>();
                Point a = new Point();
                Point b = new Point();
                double space = 10.0; // space between two arrows with opposite direction
                double gap = 8.0;   // Gap between Memo and ArrowHead

                // Optimize Start and End points position
                if (From.X + From.W < To.X){
                    a.X = From.X + From.W; a.Y = From.Y + From.H / 2 - space;
                    b.X = To.X - gap; b.Y = To.Y + To.H / 2 - space;
                }
                else if (To.X + To.W < From.X){
                    a.X = From.X; a.Y = From.Y + From.H / 2 + space;
                    b.X = To.X + To.W + gap; b.Y = To.Y + To.H / 2 + space;
                }
                else if (From.Y + From.H < To.Y){
                    a.X = From.X + From.W / 2 + space; a.Y = From.Y + From.H;
                    b.X = To.X + To.W / 2 + space; b.Y = To.Y - gap;
                }
                else{
                    a.X = From.X + From.W / 2 - space; a.Y = From.Y;
                    b.X = To.X + To.W / 2 - space; b.Y = To.Y + To.H + gap;
                }

                // When "To" Memo has not been selected yet:
                if (To == From)
                {
                    a.X = From.X + From.W;  a.Y = From.Y + From.H / 2;
                    b.X = a.X + 80;         b.Y = a.Y + 0.001;
                }

                points.Add(a);      points.Add(b);
                return points;
            }
            public Point A { get { return Points()[0]; } }  // Startpoint of this Arrow
            public Point B { get { return Points()[1]; } }  // Endpoint of this Arrow


            // Calculate Arrow Length
            double Distance()
            {
                double x1 = Points()[0].X; double y1 = Points()[0].Y;
                double x2 = Points()[1].X; double y2 = Points()[1].Y;

                return Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));
            }
            public double Length { get { return Distance(); } }


            // Calculate Arrow Angle
            double Angle()
            {                
                double x1 = Points()[0].X; double y1 = Points()[0].Y;
                double x2 = Points()[1].X; double y2 = Points()[1].Y;
                double l = Length;

                return 90.0 + Math.Acos((x2 - x1) / l)
                                 * Math.Abs(y2 - y1) / (y2 - y1 + 0.0001)
                                 * 180.0 / Math.PI;
            }
            public double Theta { get { return Angle(); } }
        }


        // Class for AbsoluteLayout "MemoBoard"
        class Board
        {
            public Point Offset;
            public Point OffsetMax;
            public Point Display;

            public double OriginalW { get; set; }
            public double OriginalH { get; set; }
        }
    }
}

