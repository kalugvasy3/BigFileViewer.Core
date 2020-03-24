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


type SelectPositions(iLineBegin: int, iCharBegin : int, iLineEnd: int,  iCharEnd : int, blnAppend : Boolean) =
    
    let mutable ilineB   = iLineBegin 
    let mutable icharB   = iCharBegin
    
    let mutable ilineE   = iLineEnd
    let mutable icharE   = iCharEnd

    let mutable blnappend = blnAppend

    member x.IntLineBegin  with get() = ilineB and set(v)= ilineB <- v
    member x.IntCharBegin  with get() = icharB and set(v)= icharB <- v 

    member x.IntLineEnd  with get() = ilineE and set(v)= ilineE <- v
    member x.IntCharEnd  with get() = icharE and set(v)= icharE <- v   
    member x.BlnAppend  with get() = blnappend and set(v)= blnappend <- v   

    member x.Clear() = ilineB <- 0
                       icharB <- 0
                       ilineE <- 0
                       icharE <- 0
                       blnappend <- false

    member x.Get with get() = (ilineB,icharB, ilineE, icharE, blnappend)

    new() = SelectPositions(0, 0, 0, 0, false)
    new(iLineB, iCharB, iLineE, iCharE) = SelectPositions(iLineB, iCharB, iLineE, iCharE, false)
   


type MapOfSelectPosition(mapOfSelectedPositions : Map<int, List<SelectPositions>> ) =
   
    let mutable mapLoSPbyLine = mapOfSelectedPositions

    member x.MapOfSP with get()= mapLoSPbyLine and set(v) = mapLoSPbyLine <- v
    member x.Empty() = mapLoSPbyLine <- Map.empty //Map.empty.Add(-1, new List<SelectPositions>())

    member x.IsEmpty() = mapLoSPbyLine.IsEmpty
    
    member x.Get(i: int)  =  let sp = mapLoSPbyLine.TryFind(i)  // None
                             match sp with
                             | Some x -> x
                             | None -> new List<SelectPositions>()                         
   
    member x.Add(i: int, lsp : List<SelectPositions>) = do mapLoSPbyLine <- mapLoSPbyLine.Add(i,lsp)
    member x.Add(i: int, iLineB : int, iCharB : int, iLineE : int, iCharE : int, blnAppenD) = 
                 let mutable lsp = new List<SelectPositions>()
                 do lsp.Add(new SelectPositions(iLineB, iCharB, iLineE, iCharE, blnAppenD))
                 do mapLoSPbyLine <- mapLoSPbyLine.Add(i,lsp)              

    member x.Update(i: int, lsp : List<SelectPositions>) = do mapLoSPbyLine <- mapLoSPbyLine.Add(i, lsp) 

    member x.Remove(i:int) = mapLoSPbyLine <- mapLoSPbyLine.Remove(i)
    new() = MapOfSelectPosition(Map.empty.Add(-1, new List<SelectPositions>()))
                   
