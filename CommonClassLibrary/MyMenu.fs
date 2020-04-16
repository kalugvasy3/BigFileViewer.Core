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

    let grdMyMenu : Grid = this.Content?grdMyMenu 

    let mutable mouseMoveArea: TreeViewItem  = this.Content?mouseMoveArea
    let mutable treeOpenFile: TreeViewItem  = this.Content?openFile 
    let mutable treeCopySelected: TreeViewItem  = this.Content?copySelected 
    let mutable treeFindWindow: TreeViewItem  = this.Content?findWindow 
    let mutable treeGoTo: TreeViewItem  = this.Content?goTo 
    let mutable txtGoTo: TextBox = this.Content?txtGoTo
    let mutable treeStopAllThread: TreeViewItem  = this.Content?stopAllThread
    let mutable treeExit: TreeViewItem  = this.Content?exit
    
    let mutable tr : TranslateTransform = this.Content?tr

    let mutable hostUserControl : UserControl = new UserControl()
    let mutable canvasMain : Canvas = null

    let mutable prev : System.Windows.Point = new System.Windows.Point()

    let mouseLeftDown(e : MouseButtonEventArgs) = 
            prev <- e.GetPosition(canvasMain)
            this.Visibility <- Visibility.Collapsed



    let mouseRightDown(e : MouseButtonEventArgs) = 
            do this.Visibility <- Visibility.Visible
            let pnt = Mouse.GetPosition(canvasMain)

            let myAH = this.ActualHeight
            let myAW = this.ActualWidth

            if pnt.X <= canvasMain.ActualWidth / 2.0 && pnt.Y <= canvasMain.ActualHeight
            then  tr.X <- pnt.X
                  tr.Y <- pnt.Y
            
            if pnt.X <= canvasMain.ActualWidth / 2.0 && pnt.Y > canvasMain.ActualHeight / 2.0
            then  tr.X <- pnt.X
                  tr.Y <- pnt.Y - myAH //- this.Margin.Top / 2.0
            
            if pnt.X > canvasMain.ActualWidth / 2.0 && pnt.Y <= canvasMain.ActualHeight / 2.0
            then  tr.X <- pnt.X - myAW //- this.Margin.Left / 2.0
                  tr.Y <- pnt.Y
            

            if pnt.X > canvasMain.ActualWidth / 2.0 && pnt.Y > canvasMain.ActualHeight / 2.0
            then  tr.X <- pnt.X - myAW - this.Margin.Left / 2.0
                  tr.Y <- pnt.Y - myAH - this.Margin.Top / 2.0


    
    let myMenuMove(e : MouseEventArgs) = 
                                         match e.RightButton with
                                         | MouseButtonState.Pressed -> 
                                                 let pos = e.GetPosition(canvasMain)
                                                 do tr.X  <-  pos.X - this.ActualWidth/ 2.0  
                                                 do tr.Y  <-  pos.Y - this.ActualHeight / 12.0  

                                         | _ -> ignore()
    

     
    do grdMyMenu.MouseMove.Add(fun e -> myMenuMove(e)) 
    

    let initMenu() = do this.Visibility <- Visibility.Collapsed 
                     do canvasMain <- (hostUserControl.Content)?canvasMain 
                     do canvasMain.MouseLeftButtonDown.Add(fun e -> mouseLeftDown(e))
                     do canvasMain.MouseRightButtonDown.Add(fun e -> mouseRightDown(e))
                     
                     do this.MouseEnter.Add(fun _ -> Mouse.SetCursor(Cursors.Arrow) |> ignore)
                     do this.MouseLeave.Add(fun _ -> Mouse.SetCursor(Cursors.IBeam) |> ignore)
                     do this.HorizontalAlignment <- HorizontalAlignment.Left
                     do this.VerticalAlignment <- VerticalAlignment.Top



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

    
    do treeOpenFile.MouseDoubleClick.Add(fun e -> openFile(e))  // Command openFile 

    do txtGoTo.MouseDoubleClick.Add(fun _ -> goTo())
    //do txtGoTo.TextChanged.Add(fun _ -> goTo())
    do treeGoTo.MouseDoubleClick.Add(fun _ -> goTo())
   
    do treeFindWindow.MouseLeftButtonUp.Add(fun _ -> eventMenu.Trigger("Find", [|""|]))  // Open Find Window

    do treeCopySelected.MouseLeftButtonUp.Add(fun _ -> eventMenu.Trigger("Copy", [|""|])) 
    do treeStopAllThread.MouseLeftButtonUp.Add(fun _ -> eventMenu.Trigger("StopAll", [|""|])) 

    do treeExit.MouseDoubleClick.Add(fun _ -> eventMenu.Trigger("Exit", [|""|]))        // Command Exit      


    [<CLIEvent>]
    member x.EventMenu =  eventMenu.Publish

    [<CLIEvent>]
    member x.EventGoTo =  eventGoTo.Publish

    member x.HostUserControl with set(v) = hostUserControl<- v
                                           do initMenu()

                                           


