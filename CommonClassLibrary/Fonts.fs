namespace CommonClassLibrary

open System
open System.Windows
open System.Windows.Controls
open System.Windows.Controls.Primitives

open System.Windows.Media



type Fonts()  = 

    let mutable coeffFont_Widh = 1.819
    let mutable coeffFont_High = 0.853 
    let mutable tb_FontSize = 14.0
    let mutable tb_FontFamily = new FontFamily("Consolas") 

    member x.CoeffFont_Widh with get()= coeffFont_Widh and set(v)=coeffFont_Widh <-v 
    member x.CoeffFont_High with get()= coeffFont_High and set(v)=coeffFont_High <-v 
    member x.Tb_FontSize    with get()= tb_FontSize and set(v)=tb_FontSize <-v 
    member x.Tb_FontFamily  with get()= tb_FontFamily and set(v)=tb_FontFamily <-v  



//private void initComboFamilyFonts()
// {
//     string[,] strFontALL;

//     strFontALL = new string[,] { 
//                                     {"Courier New",  "1.665",   "0.884"}, 
//                                     {"BatangChe",    "1.9976",  "0.87"},  
//                                     {"Consolas",     "1.8166",  "0.853"}, 
//                                     {"DFKai-listSB" ,    "1.997",  "0.834"},
//                                     {"DotumChe",    "1.9979",  "0.87"},                                                
//                                  //   {"FangSong" ,    "1.994",  "0.88"},                                                  
//                                  //   {"GungsuhChe" ,  "1.994",  "0.88"},                                              
//                                     {"KaiTi",        "1.9979",  "0.877"},
                                   
//                                     {"Lucida Console", "1.659", "1.00"},
//                                     {"Lucida Sans Typewriter", "1.6569",  "0.853"}, //
//                                     {"Letter Gothic Std",  "1.665","0.823"},    
                                     
//                                 //    {"MingLiU",  "1.995",  "0.83"}, 
//                                 //    {"MingLiU_HKSCS",  "1.995",  "0.83"},
//                                     {"MingLiU_HKSCS-ExtB",  "1.997",  "0.832"},
//                                     {"MingLiU-ExtB",  "1.997",  "0.834"},
                                     
//                                     {"NSimSun",  "1.999",  "0.877"}, //
                                    
//                                     {"OCR A","1.655",  "0.967"},
//                                     {"OCR A Std",  "1.387",  "0.835"}, 

//                                     {"Orator Std",  "1.6642",  "0.75"},
//                                     {"Segoe UI Mono",  "1.6646",  "0.752"},
//                                 //    {"SimHei",  "1.995",  "0.87"},    
//                                     {"Simplified Arabic Fixed","1.6646","0.917"},
//                                 //    {"SimSun","1.995","0.87"},      
//                                     {"SimSun-ExtB" ,"1.997","1.00"}
//                                 };

//     for (int i = 0; i < strFontALL.GetLength(0); i++)
//     {

//         ComboBoxItem cbi = new ComboBoxItem();
//         selectedFonts sF = new selectedFonts();

//         sF.strFontName = strFontALL[i, 0];
//         sF.dHoriz = Double.Parse(strFontALL[i, 1].Trim());
//         sF.dVert = Double.Parse(strFontALL[i, 2].Trim());

//         // inherent from Label
//         sF.Text = strFontALL[i, 0];
//         sF.FontFamily = new FontFamily(strFontALL[i, 0]);
//         sF.Visibility = System.Windows.Visibility.Visible;

//         sF.FontSize = 13;
//         sF.Height = 21;

//         cbi.Content = sF;  // <- this item will be storing on comboBox
//         cbi.ToolTip = strFontALL[i, 0];
//         cbi.FontFamily = new FontFamily(strFontALL[i, 0]);

//         foreach (FontFamily fontFamily in Fonts.SystemFontFamilies)
//         {
//             if (fontFamily.ToString() == sF.FontFamily.ToString())
//             {
//                 comboFontFamily.Items.Add(cbi);
//             }
//         }
//     }
//     comboFontFamily.SelectedIndex = 0;
//     comboFontSize.SelectedIndex = 2;
// }



//private void initComboColor()
// {
//     // populate colors drop down (will work with other kinds of list controls) 

//     Type colors = typeof(System.Drawing.Color);
//     PropertyInfo[] colorInfo = colors.GetProperties(BindingFlags.Public | BindingFlags.Static);

//     foreach (PropertyInfo info in colorInfo)
//     {

//         ComboBoxItem cbi = new ComboBoxItem();
//         selectedColors sC = new selectedColors();

//         sC.VerticalAlignment = System.Windows.VerticalAlignment.Center;
//         sC.strColor = info.Name;
//         sC.Text = info.Name;
//         sC.Visibility = System.Windows.Visibility.Visible;
//         sC.FontFamily = new FontFamily("Courier New");
//         sC.FontSize = 13;
//         sC.Height = 21;


//         if (info.Name != "Transparent")
//         {
//             sC.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(info.Name));
//         }
//         else
//         {
//             sC.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Black"));
//         }
//         cbi.ToolTip = sC.strColor;
//         cbi.Content = sC;

//         comboColor.Items.Add(cbi);
//     }

//     comboColor.SelectedIndex = 80;
// }


