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


type SelectedPositions(iCharBegin : int, iCharEnd : int) =

    let mutable icharB   = iCharBegin 
    let mutable icharE   = iCharEnd

    member x.IntCharBegin  with get() = icharB and set(v)= icharB <- v 
    member x.IntCharEnd with get() = icharE and set(v)= icharE <- v   

    member x.Clear() = icharB   <- 0
                       icharE   <- 0

    member x.SelectedPositions(iCharB, iCharE) =
                                   icharB <- iCharB;
                                   icharE <- iCharE;

    new() = SelectedPositions(0, 0)
    new(iCharB, iCharE, iLine) = SelectedPositions(iCharB, iCharE)
   


type MapOfSelectedPosition(mapOfSelectedPositions : Map<int, SelectedPositions> ) =
   
    let mutable mapSPbyLine = mapOfSelectedPositions

    member x.MapOfSP with get()= mapSPbyLine and set(v) = mapSPbyLine <- v
    member x.Empty() = mapSPbyLine <- Map.empty
    member x.Get(i: int)  = mapSPbyLine.TryFind(i)  // None
    member x.Add(i: int, sp : SelectedPositions) = mapSPbyLine.Add(i,sp) 
    member x.Add(i: int, iCharBegin : int, iCharEnd : int) = mapSPbyLine.Add(i,new SelectedPositions(iCharBegin, iCharEnd))

    member x.Update(i: int, iCharBegin : int, iCharEnd : int) = mapSPbyLine.Remove(i) |> ignore
                                                                mapSPbyLine.Add(i,new SelectedPositions(iCharBegin, iCharEnd))
    member x.Remove(i:int) = mapSPbyLine.Remove(i)
    
    new() = MapOfSelectedPosition(Map.empty.Add(0, new SelectedPositions(0, 0)))
                   
