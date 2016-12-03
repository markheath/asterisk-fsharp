module Mvvm
    open System
    open System.Windows
    open System.Windows.Input
    open System.Windows.Markup
    open System.Windows.Media
    open System.ComponentModel

    let checkCollisionPoint (point:Point) (control:UIElement) =
        let transformPoint = control.RenderTransform.Inverse.Transform(point)
        VisualTreeHelper.HitTest(control, transformPoint) <> null

    let loadXaml path =
        let uri = new Uri(path, UriKind.Relative)
        let stream = Application.GetResourceStream(uri).Stream
        XamlReader.Load stream

    let runApp rootElement =
        let app = new Application()
        app.Run rootElement

    type ViewModelBase() =
        let propertyChangedEvent = new DelegateEvent<PropertyChangedEventHandler>()
        
        interface INotifyPropertyChanged with
            [<CLIEvent>]
            member x.PropertyChanged = propertyChangedEvent.Publish

        member x.OnPropertyChanged propertyName = 
            propertyChangedEvent.Trigger([| x; new PropertyChangedEventArgs(propertyName) |])
 
    type RelayCommand (canExecute:(obj -> bool), action:(obj -> unit)) =
        let event = new DelegateEvent<EventHandler>()
        interface ICommand with
            [<CLIEvent>]
            member x.CanExecuteChanged = event.Publish
            member x.CanExecute arg = canExecute(arg)
            member x.Execute arg = action(arg)
     
    type DelegateCommand (action:(obj -> unit)) =
        let event = new DelegateEvent<EventHandler>()
        let mutable canExecute = true
        interface ICommand with
            [<CLIEvent>]
            member x.CanExecuteChanged = event.Publish
            member x.CanExecute arg = canExecute
            member x.Execute arg = action(arg)
        member x.SetCanExecute ce = 
            canExecute <- ce
            event.Trigger([|x; EventArgs.Empty|])