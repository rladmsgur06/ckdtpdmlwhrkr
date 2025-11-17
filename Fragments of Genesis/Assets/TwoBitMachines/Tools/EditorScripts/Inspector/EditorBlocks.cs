#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TwoBitMachines.Editors
{
        public static class EditorBlocks
        {

        }

        public class EditorBase
        {

        }

        public class Setup : EditorBase
        {

                public static void Use ()
                {
                        //   if (FoldOut.space) Layout.VerticalSpacing (1);
                        //   barStart = Layout.CreateRectAndDraw (width: Layout.longInfoWidth - xAdjust, height: height, xOffset: -11 + xAdjust, texture : texture, color : barColor);
                        //   barEnd = new Rect (barStart) { x = barStart.x + barStart.width - 6 };
                        //   startX = startXOrigin = barStart.x;
                }
        }

        public interface Build
        {
                public void Use ();
        }

        //   public struct BR : Build
        //   {
        //           public string icon;

        //           public BR (string icon)
        //           {
        //                   this.icon = icon;
        //           }

        //           public void Use ( )
        //           {
        //                   ChainBar.ButtonRight (icon, Color.white);
        //           }
        //   }

}
#endif
