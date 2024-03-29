﻿module MainWindow

open System 
open System.Windows 
open System.IO
open System.Windows.Markup

open System.Windows.Controls 

open System.Reflection
open System.Windows.Media.Imaging

// init Main Window
let mutable this : Window = new Window()
do this.ResizeMode <- ResizeMode.CanResizeWithGrip
do this.SizeToContent <-SizeToContent.WidthAndHeight 
do this.Title <- "Big File Viewer"
do this.MinHeight <- 750.0
do this.MinWidth <- 512.0
do this.MaxHeight <- 2160.0   // limit lines per screen 200 lines
do this.MaxWidth <- 4000.0


do this.SizeToContent <- SizeToContent.WidthAndHeight

let iconUri = new Uri("pack://application:,,,/AP.ico", UriKind.RelativeOrAbsolute);
do this.Icon <-  BitmapFrame.Create(iconUri)
do this.Name <- "MainWindow" 


let mutable  ucMainWindow = new MyUserControl.BigFileViewer()
do this.Content <- ucMainWindow

do this.Loaded.Add(fun _ ->  ucMainWindow.WinHolder <- this)
do this.Unloaded.Add(fun _ -> this <- null 
                              GC.Collect())   //Environment.Exit(0)

do this.SizeChanged.Add(fun _ ->  ucMainWindow.WinHolder <- this)

//https://docs.microsoft.com/en-us/dotnet/api/system.windows.application.shutdownmode?redirectedfrom=MSDN&view=netframework-4.8#System_Windows_Application_ShutdownMode

let newApp : Application = new Application()
do newApp.ShutdownMode <- ShutdownMode.OnMainWindowClose

[<STAThread>] 
[<EntryPoint>]
do (newApp).Run(this) |> ignore

