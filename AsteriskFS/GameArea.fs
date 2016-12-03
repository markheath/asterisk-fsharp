module GameArea
    open System
    open System.Windows
    open System.Windows.Controls
    open System.Windows.Media
    open System.Windows.Shapes

    let height, width = 200.0, 300.0
    let rand = Random()
    type GameArea = {canvas:Canvas;polyline:Polyline;stars:ResizeArray<Path>}

    let create canvas =
        { canvas=canvas; polyline=Polyline(Stroke = Brushes.Yellow, StrokeThickness = 2.0); stars=ResizeArray<Path>()}

    let isCollision stars x y width height =
        if y <= 0.0 || y >= height then
            // we've hit top or bottom
            true
        elif x >= width then
            // we've hit the right edge but missed the gap
            not ((height / 2.0 + 15.0) > y && y > (height / 2.0 - 15.0))
        else
            let isStarCollision star =
                let testX = x - Canvas.GetLeft(star)
                let testY = y - Canvas.GetTop(star)
                let testPoint = Point (testX, testY)
                Mvvm.checkCollisionPoint testPoint star
            stars |> Seq.exists isStarCollision

    let addLine (canvas:Canvas) (fromX,fromY) (toX,toY) =
        let line = Line(X1 = fromX, Y1 = fromY,
                        X2 = toX, Y2 = toY,
                        Stroke = Brushes.White,
                        StrokeThickness = 2.0)
        canvas.Children.Add(line) |> ignore

    let drawBorders canvas =
        addLine canvas (0.0, 0.0) (width, 0.0) // line across top
        addLine canvas (0.0, height) (width, height) // line across botom
        addLine canvas (width, 0.0) (width, height / 2.0 - 15.0)
        addLine canvas (width,height / 2.0 + 15.0) (width, height)

    let randf min max = min + rand.NextDouble() * (max - min)

    let redrawScreen { canvas = canvas; polyline = polyline; stars = stars } level =
        canvas.Children.Clear()
        drawBorders canvas

        stars.Clear()
        for n in [1..level*3] do
            let shape = if rand.Next(0,2) = 1 then "star.xaml" else "tree.xaml"
            let star = Mvvm.loadXaml shape :?> Path
            stars.Add star
            Canvas.SetLeft(star, float(rand.Next(10, int(width) - 10)))
            Canvas.SetTop(star, float(rand.Next(2, int(height) - 10)))
            canvas.Children.Add star |> ignore
    
        polyline.Points.Clear()
        canvas.Children.Add(polyline) |> ignore

    let addPosition ga (x,y) =
        ga.polyline.Points.Add(Point(x, y))