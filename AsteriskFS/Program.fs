// better:
// GameState record type: level, xPos, yPos, stars, direction, isAlive
// New gamestates by tick, by direction change

// IDEAS:
// Refactor to Record types and modules
// More functional game engine
// Trees as well as stars
// A sleigh?
// Trail only to use minimal points
// Snow! http://blog.jerrynixon.com/2013/12/you-can-make-it-snow-in-xaml-here-ill.html

open System
open System.Windows
open System.Windows.Controls
open System.Windows.Input
open System.Windows.Threading

type Direction = Up | Down
type GameState = { xPos:float; yPos:float; direction:Direction; currentLevel:int}
type RenderAction = NewPoint | NewLevel | GameOver
type GameEvent = Tick | ChangeDirection of Direction

type MainViewModel (xaml:Window) as this = 
    inherit Mvvm.ViewModelBase()

    let mutable highScore = HighScore.load ()

    let mutable gameState = { xPos=0.0; yPos=0.0; direction=Down;currentLevel=0}
    
    let timer = DispatcherTimer(Interval = TimeSpan.FromMilliseconds(20.0))

    let gameArea = GameArea.create(xaml.FindName("gameCanvas") :?> Canvas)
    let highScorePresenter = xaml.FindName("highScorePresenter") :?> ContentPresenter
    let message = GameOver.GameOver()
    do 
        highScorePresenter.Content <- message.Xaml
    let newGameCommand = new Mvvm.DelegateCommand ((fun _ -> this.NewGame())) 

    let isHighScore() = gameState.currentLevel > highScore.Level

    let renderGameOver() = 
        let onDone newHighScoreName =
            message.Hide()
            if isHighScore() then
                highScore <- {Name=newHighScoreName;Level=gameState.currentLevel}
                newGameCommand.SetCanExecute true
                this.OnPropertyChanged "Record"
                HighScore.save highScore
            else
                this.NewGame()
               
        timer.Stop()
        if isHighScore() then
            message.Show "High Score" onDone true
        else
            message.Show "Game Over, Play Again?" onDone false


    let handleTick colTest gs =
        if gs.currentLevel = 0 then
            NewLevel, { currentLevel = 1; xPos = 0.0; yPos = GameArea.height / 2.0; direction = Down }
        else
            let newX = gs.xPos + 1.0
            let newY = gs.yPos + match gs.direction with | Up -> -1.0 | Down -> 1.0

            let crash = colTest newX newY GameArea.width GameArea.height
            if crash then
                GameOver, gs
            else if gs.xPos >= GameArea.width then
                NewLevel, { currentLevel = gs.currentLevel + 1; xPos = 0.0; yPos = GameArea.height / 2.0; direction = Down }
            else
                NewPoint,  { gs with  xPos = newX; yPos = newY } 

    let renderNewLevel gs =
        GameArea.redrawScreen gameArea gs.currentLevel
        GameArea.addPosition gameArea (gs.xPos, gs.yPos)
        this.OnPropertyChanged "Level" // TODO: this line needs to move now

    let onTick _ =
        let renderAction, gs = handleTick (GameArea.isCollision gameArea.stars) gameState
        gameState <- gs
        match renderAction with
        | NewPoint -> 
            GameArea.addPosition gameArea (gs.xPos, gs.yPos)
        | GameOver -> 
            renderGameOver()
        | NewLevel -> 
            renderNewLevel gs


    let onGameEvent gameEvent =
        match gameEvent with
        | Tick -> onTick ()
        | ChangeDirection Up -> gameState <- { gameState with direction = Up }
        | ChangeDirection Down -> gameState <- { gameState with direction = Down }

    let isSpace (a:KeyEventArgs) = a.Key = Key.Space

    do
        [ timer.Tick |> Observable.map (fun _ -> Tick) 
          xaml.KeyDown |> Observable.filter isSpace |> Observable.map (fun _ -> ChangeDirection Up)
          xaml.KeyUp |> Observable.filter isSpace |> Observable.map (fun _ -> ChangeDirection Down)
          xaml.MouseLeftButtonDown |> Observable.map (fun _ -> ChangeDirection Up)
          xaml.MouseLeftButtonUp |> Observable.map (fun _ -> ChangeDirection Down)          
        ]
        |> List.reduce Observable.merge 
        |> Observable.subscribe onGameEvent
        |> ignore // TODO - later we need to track the disposables

    do 
        xaml.Closing.Add (fun _ -> timer.Stop())

    member x.Level 
        with get () = sprintf "Level %d" gameState.currentLevel

    member x.Record
        with get () = HighScore.describe highScore

    member x.NewGameCommand 
        with get() = newGameCommand

    member x.NewGame() =
        gameState <- { xPos=0.0; yPos=0.0; direction=Down;currentLevel=0}
        newGameCommand.SetCanExecute(false)
        // need a way to inject a new level
        //x.NewLevel 1
        // for now, starting the timer will cause us to realise we need new level
        timer.Start()

[<STAThread>]
[<EntryPoint>]
let main(_) =
    let mainWindow = Mvvm.loadXaml "Asterisk.xaml" :?> Window
    mainWindow.DataContext <- new MainViewModel (mainWindow)
    Mvvm.runApp mainWindow
