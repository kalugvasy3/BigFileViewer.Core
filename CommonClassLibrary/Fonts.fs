namespace CommonClassLibrary

open System
open System.Windows
open System.Windows.Controls
open System.Windows.Controls.Primitives

open System.Windows.Media



type Fonts()  = 

    let mutable coeffFont_Widh = 1.824   // "1.8166"   1.830
    let mutable coeffFont_High = 0.868   // "0.853"    0.880
    let mutable tb_FontSize = 14.0
    let mutable tb_FontFamily = new FontFamily("Consolas") 

    member x.CoeffFont_Widh with get()= coeffFont_Widh and set(v)=coeffFont_Widh <-v 
    member x.CoeffFont_High with get()= coeffFont_High and set(v)=coeffFont_High <-v 
    member x.Tb_FontSize    with get()= tb_FontSize and set(v)=tb_FontSize <-v 
    member x.Tb_FontFamily  with get()= tb_FontFamily and set(v)=tb_FontFamily <-v  

