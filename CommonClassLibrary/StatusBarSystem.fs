namespace CommonClassLibrary

open System
open System.Collections.ObjectModel

open System.Windows
open System.Windows.Controls
    
open  System.Windows
open  System.Windows.Shapes
open  System.Windows.Controls
open  System.Windows.Controls.Primitives
open  System.Windows.Input
open  System.Windows.Media
open  System.Windows.Media.Imaging
open  System.Windows.Threading

open System.IO
open System.Windows.Markup
open System.Xaml
open System.Reflection
open System.Windows.Media

open Utilities

open Microsoft.FSharp.Control
open System.Diagnostics


type StatusBarSystem()  as this = 
    inherit  UserControl()

    //do this.Content <- contentAsXamlObject("StatusBarSystem") // Load XAML
    do this.Content <- contentAsXamlObjectFromAssembly("CommonClassLibrary","StatusBarSystem") // Load XAML


    let mutable statusSystem : StatusBar =  this.Content?statusSystem 
    let mutable statusSystemBar : StatusBarItem = this.Content?statusSystemBar
    let mutable statusFile : StatusBarItem = this.Content?statusFile
    let mutable statusDocSizeBar : StatusBarItem = this.Content?statusDocSizeBar
   
    let mutable prgStatus : ProgressBar = this.Content?prgStatus
    
    let mutable fullFileName : String = ""
    let mutable numberTotalLines : int = 0
 

    let mutable longTotalDocSize : System.Int64 = 0L
    let mutable scaleCurrent : float = 1.0

    let systemInfo() =  do GC.Collect()
                        let intUsedMemory : int64  =  System.GC.GetGCMemoryInfo().MemoryLoadBytes 
                        let intUsedMemoryCurrentProcess : int64 = Process.GetCurrentProcess().WorkingSet64
                        let totalAvailable =  System.GC.GetGCMemoryInfo().TotalAvailableMemoryBytes                        
                        let mutable intOS = 32
                        if Environment.Is64BitOperatingSystem then intOS <- 64                       
                        do statusSystemBar.Content <- "OS: " + Environment.OSVersion.ToString() 
                                                             + " Scale " + (Convert.ToInt32(scaleCurrent * 100.0)).ToString() + "%" 
                                                             + " Used Memory " + ((float)intUsedMemoryCurrentProcess / 1024.0 / 1024.0).ToString("0,0.00") + " MByte," 
                                                             + " Total Memory Loaded " + ((float)intUsedMemory/ 1024.0 / 1024.0).ToString("0,0") + " MByte,"
                                                             + " (Total Memory " + ((float)totalAvailable/ 1024.0 / 1024.0 / 1024.0).ToString("0,0") +  " GByte)";

                        do statusFile.Content <- "  lines - " + numberTotalLines.ToString("0,0")
                        do statusDocSizeBar.Content <- " file - " + fullFileName + "  ( " + longTotalDocSize.ToString("0,0") + " bytes ) " 
 
    //let systemInfo() = let myComputer =  new Microsoft.VisualBasic.Devices.ComputerInfo() 
    //                   let totalAvailable  = myComputer.AvailablePhysicalMemory 
    //                   let mutable intOS = 32
    //                   if Environment.Is64BitOperatingSystem then intOS <- 64
    //                   let intTotalMemory = System.GC.GetTotalMemory(false)
    //                   do statusSystemBar.Content <- "OS:X" + intOS.ToString() + " Scale " + (Convert.ToInt32(scaleCurrent * 100.0)).ToString() + "%  Used Memory - " + intTotalMemory.ToString("0,0") + " ( Available " + totalAvailable.ToString("0,0") + " )";
    //                   do statusFile.Content <- "  lines - " + numberTotalLines.ToString("0,0")
    //                   do statusDocSizeBar.Content <- " file - " + fullFileName + "  ( " + longTotalDocSize.ToString("0,0") + " bytes ) " 


    do systemInfo()


    // Add Event - Selection Changed
    let eventStatusSystem = new Event<MouseEventArgs>() 
    do statusSystem.MouseLeftButtonDown.Add(fun e ->
                                                 GC.Collect()
                                                 System.Threading.Thread.Sleep(40)
                                                 do systemInfo() 
                                                 eventStatusSystem.Trigger(e)
                                           )

    [<CLIEvent>]
    member x.EventStatusSystem  =  eventStatusSystem.Publish 

    member x.LongTotalDocSize  with set(v) = longTotalDocSize <- v
                                             do systemInfo()
    
    member x.FullFileName  with get() = fullFileName and  set(v) = ( fullFileName <- v 
                                                                     do systemInfo())

    member x.NumberTotalLines  with get() = numberTotalLines and  set(v) = ( numberTotalLines <- v 
                                                                             do systemInfo())
    
    member x.SystemInfo() = systemInfo()

    member x.ScaleNewValue  with set(v) = (scaleCurrent <- v
                                           do systemInfo())

    member x.PrgStatusValue with set(v) = (prgStatus.Value <- v
                                           do systemInfo())


