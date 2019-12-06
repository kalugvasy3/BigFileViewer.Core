namespace CommonClassLibrary

open System
open System.Windows
open System.Windows.Controls
open System.Windows.Controls.Primitives
open System.IO
open System.Drawing
open System.Windows.Markup
open System.Windows.Input
open System.Reflection
open System.Threading.Tasks
open System.Threading

open System.Windows.Media
open System.Windows.Media.Imaging
open System.Windows.Input


open Utilities 
open System.Windows.Threading
open System.Collections.ObjectModel


//https://msdn.microsoft.com/en-us/library/system.windows.input.touch(v=vs.110).aspx

type Touch() = 

    let eventTouch = new Event<float*float*float>() 
    let mutable myCanvas : Canvas = null
    let mutable myFont  = new  CommonClassLibrary.Fonts()

    let mutable lScale = 1.0

    let mutable touchPointsPrimary = Array2D.create 2 2 0.0
    let mutable touchPoints        = Array2D.create 2 2 0.0 

    let myScale() = let primary = Math.Sqrt((touchPointsPrimary.[0 , 0] - touchPointsPrimary.[1 , 0]) ** 2.0 +     // deltaX
                                            (touchPointsPrimary.[0 , 1] - touchPointsPrimary.[1 , 1]) ** 2.0 )     // deltaY
                    let secondary = Math.Sqrt((touchPoints.[0 , 0] - touchPoints.[1 , 0]) ** 2.0 +     // deltaX
                                              (touchPoints.[0 , 1] - touchPoints.[1 , 1]) ** 2.0 )     // deltaY
                    do lScale <- ((primary / secondary - 1.0) * 100.0 ) % 5.0  // 
                    ignore()

    let touchFrame(sender : obj, e: TouchFrameEventArgs) = 

            let mutable blnContinue = false
            let touchPointsFromUC = e.GetTouchPoints(myCanvas)
            match touchPointsFromUC.Count with
            | 0 -> ignore()
            | 1 -> ignore()   // same like mouse - selecting etc.
            | 2 -> for i = 0 to 1 do
                       let tp = touchPointsFromUC.[i]
                       match tp.Action with        
                       | TouchAction.Down  -> touchPointsPrimary.[i , 0] <- tp.Position.X   // This is the first (primary) touch point. Just record its position.
                                              touchPointsPrimary.[i , 1] <- tp.Position.Y
                                              do blnContinue <- false
                       | TouchAction.Move  -> touchPoints.[i , 0] <- tp.Position.X         // This is not the first touch point. 
                                              touchPoints.[i , 1] <- tp.Position.Y                                     
                                              do blnContinue <- true
                       | TouchAction.Up    -> myCanvas.ReleaseAllTouchCaptures()
                       | _ -> ignore()
                   
                   if blnContinue then 
                       let avrPrimaryX = touchPointsPrimary.[*, 0] |> Array.average
                       let avrPrimaryY = touchPointsPrimary.[*, 1] |> Array.average
                       let avrX = touchPoints.[*, 0] |> Array.average
                       let avrY = touchPoints.[*, 1] |> Array.average
                   
                         // use INT for round ... using like index for array of string ...   
                       let deltaX = int ((avrPrimaryX - avrX) *  myFont.CoeffFont_Widh / myFont.Tb_FontSize)
                       let deltaY = int ((avrPrimaryY - avrY) * myFont.CoeffFont_High / myFont.Tb_FontSize )

                       if Math.Abs(deltaX) > 0 then  do touchPointsPrimary.[* , 0] <- touchPoints.[* , 0]
                       if Math.Abs(deltaY) > 0 then  do touchPointsPrimary.[* , 1] <- touchPoints.[* , 1]
                       do myScale()
                       do eventTouch.Trigger(float deltaX , float deltaY, 0.0) // lScale just show direction increase/decries 

                   ignore()

            | 3 -> ignore()
            | _ -> ignore()

    [<CLIEvent>]
    member x.EventTouch =  eventTouch.Publish
    member x.MyFont with get() = myFont and set(v) = myFont <- v
    member x.MyCanvas with get() = myCanvas and set(v) = myCanvas <- v

    member x.TouchFrame(sender : obj, e: TouchFrameEventArgs) = touchFrame(sender, e)




