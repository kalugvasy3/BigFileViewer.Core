namespace CommonClassLibrary

open System
open System.Collections.Generic;

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

open Microsoft.FSharp.Control
open System.Diagnostics


type SelectedPositions(iCharBegin : int, iCharEnd : int, iUIE : Nullable<int>) =

    let mutable icharB   = iCharBegin 
    let mutable icharE   = iCharEnd
    let mutable id_UIElement = iUIE

    member x.IntCharBegin  with get() = icharB and set(v)= icharB <- v 

    member x.IntCharEnd with get() = icharE and set(v)= icharE <- v 

    member x.SelectedPositions() = icharB   <- 0
                                   icharE   <- 0
                                   id_UIElement <- Nullable()

    member x.SelectedPositions(iCharB, iCharE, idUIElement) =
                                   icharB <- iCharB;
                                   icharE <- iCharE;
                                   id_UIElement <- idUIElement

    new() = SelectedPositions(0, 0, Nullable<int>(0))
   

   
type SelectedPositionsCurrent(icharCurrent,ilineCurrent,iDeltaX, iDeltaY) =
   
    let mutable i_charCurrent   = icharCurrent 
    let mutable i_lineCurrent   = ilineCurrent
    let mutable i_DeltaX   = iDeltaX 
    let mutable i_DeltaY   = iDeltaY
   
    member x.IntCharCurrent  with get() = i_charCurrent and set(v)= i_charCurrent <- v   
    member x.IntLineCurrent with get() = i_lineCurrent and set(v)= i_lineCurrent <- v  
    member x.IntDeltaX  with get() = i_DeltaX and set(v)= i_DeltaX <- v   
    member x.IntDeltaY  with get() = i_DeltaY and set(v)= i_DeltaY <- v       
      
    member x.SelectedPositionsCurrent(sp : SelectedPositionsCurrent) = 
            i_charCurrent <- sp.IntCharCurrent
            i_lineCurrent <- sp.IntLineCurrent
            i_DeltaX <- sp.IntDeltaX
            i_DeltaY <- sp.IntDeltaY

    member x.SelectedPositionsCurrent() = 
            i_charCurrent <- 0
            i_lineCurrent <- 0
            i_DeltaX <- 0
            i_DeltaY <- 0
   
    member x.SelectedPositionsCurrent(iLineCurrent, iCharCurrent) = 
            i_charCurrent <- iCharCurrent
            i_lineCurrent <- iLineCurrent

    new() = SelectedPositionsCurrent(0, 0, 0, 0)



type ListOfSelectedPositionInLine(listSelectedPositions : List<SelectedPositions>) =
   
    let mutable listSPinLine = listSelectedPositions

    member x.ListSPinLine  with get() = listSPinLine and set(v)= listSPinLine <- v  

    member x.ListOfSelectedPositionInLine() = listSPinLine.Clear()

    member x.ListOfSelectedPositionInLine(sp : SelectedPositions) =
             listSPinLine.Add(sp); 

    member x.ListOfSelectedPositionInLine(iCharBegin, iCharEnd, iUIE) = 
             listSPinLine.Add(SelectedPositions(iCharBegin, iCharEnd, iUIE));

    new() = ListOfSelectedPositionInLine(new List<SelectedPositions>())  

