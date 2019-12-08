namespace CommonClassLibrary


open System
open System.Collections.ObjectModel

open System.Windows
open System.Windows.Controls
    
open  System.Windows
open  System.Windows.Shapes
open  System.Windows.Controls
open  System.Windows.Input
open  System.Windows.Media
open  System.Windows.Media.Imaging
open  System.Windows.Threading
open  System.Threading
open System.IO
open System.Windows.Markup
open System.Xaml
open System.Reflection
open System.Windows.Media
open Utilities
open Microsoft.Win32;


open CommonClassLibrary

open Microsoft.FSharp.Control

type  MyMenu()  as this = 
    inherit  UserControl()

    //do this.Content <- contentAsXamlObject("MyMenu") // Load XAML
    do this.Content <- contentAsXamlObjectFromAssembly("CommonClassLibrary","MyMenu") :?> UserControl // Load XAML


    let eventMenu = new Event<string*string[]>() 
    let eventGoTo = new Event<int*int>()

    let mutable menuOpen : MenuItem = this.Content?menuOpen
    let mutable menuExit : MenuItem = this.Content?menuExit
    
    let mutable txtFindNext : TextBox = this.Content?txtFindNext
    let mutable btnFindNext : Button = this.Content?btnFindNext 

    let mutable txtGoTo : TextBox = this.Content?txtGoTo 
    let mutable btnGoTo : Button = this.Content?btnGoTo  

    let mutable hostUserControl : UserControl = new UserControl()

    let mutable canvasMain : Canvas = null

    
    let mutable tr = new System.Windows.Media.TranslateTransform()       
    do this.RenderTransform <- tr 

    let mutable prev : System.Windows.Point = new System.Windows.Point()

    let mouseLeftDown(e : MouseButtonEventArgs) = 
            prev <- e.GetPosition(canvasMain)
            this.Visibility <- Visibility.Collapsed


    let mouseRightDown(e : MouseButtonEventArgs) =            
            match canvasMain.AllowDrop with
            | true -> prev <- e.GetPosition(canvasMain)
                      this.Visibility <- Visibility.Visible

                      do tr.X  <-  prev.X - canvasMain.ActualWidth  / 2.0  
                      do tr.Y  <-  prev.Y - canvasMain.ActualHeight / 2.0  
            | _ -> ignore()

    
    let myMenuMove(e : MouseEventArgs) = match e.LeftButton with
                                         | MouseButtonState.Pressed -> 
                                                 let pos = e.GetPosition(canvasMain)
                                                 do tr.X  <-  pos.X - canvasMain.ActualWidth  / 2.0  
                                                 do tr.Y  <-  pos.Y - canvasMain.ActualHeight / 2.0  

                                         | _ -> ignore()
    

     
    do this.MouseMove.Add(fun e -> myMenuMove(e)) 


    let initMenu() = do this.Visibility <- Visibility.Collapsed 
                     do canvasMain <- (hostUserControl.Content)?canvasMain 
                     do canvasMain.MouseLeftButtonDown.Add(fun e -> mouseLeftDown(e))
                     do canvasMain.MouseRightButtonDown.Add(fun e -> mouseRightDown(e))


    let openFile(e : RoutedEventArgs) = 
        
        let  openFileDialog = new OpenFileDialog()
        do openFileDialog.Filter <- "All files (*.*)|*.*" 
        
        let result : Nullable<bool> = openFileDialog.ShowDialog()
        if result.HasValue then
            let strF = openFileDialog.FileNames
            match strF <> [||] with
            | true -> eventMenu.Trigger("Open", strF)
            | _ -> ignore()


    let goTo() =  let t = txtGoTo.Text.Split(',')
                  let mutable iY = ref (-1)
                  let mutable iX = ref (-1)
                 
                  let out = match t.Length with
                             | 1 -> if Int32.TryParse(t.[0], iY) then ( !iY , -1 ) else ( -1 , -1 ) 
                             | 2 -> match Int32.TryParse(t.[0], iY) , Int32.TryParse(t.[1], iX)  with
                                    | true, true ->  (!iY , !iX)
                                    | true, false -> (!iY , -1)
                                    | false, true -> (-1 , !iX )
                                    | _ , _ -> (-1, -1)
                             | _ -> (-1, -1)
                  eventGoTo.Trigger(out)



    do menuOpen.Click.Add(fun e -> openFile(e))
    do menuExit.Click.Add(fun _ -> eventMenu.Trigger("Exit", [|""|]))
    
    do btnFindNext.Click.Add(fun _ -> eventMenu.Trigger("Next", [|txtFindNext.Text|]))
    do txtFindNext.TextChanged.Add(fun _ -> eventMenu.Trigger("FindChanged", [|txtFindNext.Text|]))
    do this.KeyDown.Add(fun e ->  match e.Key with
                                  | Key.Escape -> eventMenu.Trigger("Escape", [|txtFindNext.Text|])
                                  | _ -> ignore() )

    do btnGoTo.Click.Add(fun _ -> goTo())

    [<CLIEvent>]
    member x.EventMenu =  eventMenu.Publish

    [<CLIEvent>]
    member x.EventGoTo =  eventGoTo.Publish

    member x.HostUserControl with set(v) = hostUserControl<- v
                                           do initMenu()

    member x.TxtFind with get()= txtFindNext.Text
                                           


