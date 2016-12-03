module GameOver
    open System.Windows
    
    type GameOverViewModel() =
        inherit Mvvm.ViewModelBase()
        
        let mutable message = ""
        let mutable name = ""
        let mutable callback = (fun _ -> ())
        let mutable isHighScore = Visibility.Collapsed
       
        member x.OKCommand 
            with get () = Mvvm.DelegateCommand((fun _ -> callback(name)))
   
        member x.Message
            with get () = message
            and set value = 
                if message <> value then
                    message <- value
                    x.OnPropertyChanged "Message"

        member x.Name
            with get () = name
            and set value = 
                if name <> value then
                    name <- value
                    x.OnPropertyChanged "Name"

        member x.IsHighScore
            with get () = isHighScore
            and set value = 
                if isHighScore <> value then
                    isHighScore <- value
                    x.OnPropertyChanged "IsHighScore"

        member x.Callback
            with get () = callback
            and set value = callback <- value

    type GameOver() =
        let xaml = Mvvm.loadXaml "gameover.xaml" :?> FrameworkElement
        let viewModel = GameOverViewModel()
        let hide() = xaml.Visibility <- Visibility.Collapsed
        do 
            xaml.DataContext <- viewModel
            hide()
       
        member x.Show message callback isHighScore =
           viewModel.Message <- message
           viewModel.IsHighScore <- if isHighScore then Visibility.Visible else Visibility.Collapsed
           xaml.Visibility <- Visibility.Visible
           viewModel.Callback <- callback

        member x.Hide() = hide()

        member x.Xaml with get() = xaml
