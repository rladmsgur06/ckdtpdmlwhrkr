using System;
using UnityEngine.UI;

namespace UnityEngine.InputSystem.Samples.RebindUI
{
      public class SetNewInputIcons : MonoBehaviour
      {
            [SerializeField] public GamepadIcons icons;

            protected void OnEnable ( )
            {
                  var rebindUIComponents = transform.GetComponentsInChildren<RebindNewInput> ( );
                  foreach (var component in rebindUIComponents)
                  {
                        component.updateBindingUIEvent.AddListener (OnUpdateBindingDisplay);
                        component.UpdateBindingDisplay ( );
                  }
            }

            protected void OnUpdateBindingDisplay (GameObject labelGO, string controlPath)
            {
                  Sprite icon = icons.GetSprite (controlPath);
                  var imageComponent = labelGO.transform.parent.Find ("Icon")?.GetComponent<Image> ( );

                  if (imageComponent == null)
                  {
                        return;
                  }

                  if (icon != null)
                  {
                        labelGO?.gameObject.SetActive (false);
                        imageComponent.sprite = icon;
                        imageComponent.gameObject.SetActive (true);
                  }
                  else
                  {
                        labelGO?.gameObject.SetActive (true);
                        imageComponent.gameObject.SetActive (false);
                  }
            }

            [Serializable]
            public struct GamepadIcons
            {
                  public Sprite buttonSouth;
                  public Sprite buttonNorth;
                  public Sprite buttonEast;
                  public Sprite buttonWest;
                  public Sprite startButton;
                  public Sprite selectButton;
                  public Sprite leftTrigger;
                  public Sprite rightTrigger;
                  public Sprite leftShoulder;
                  public Sprite rightShoulder;
                  public Sprite dpad;
                  public Sprite dpadUp;
                  public Sprite dpadDown;
                  public Sprite dpadLeft;
                  public Sprite dpadRight;
                  public Sprite leftStick;
                  public Sprite rightStick;
                  public Sprite leftStickPress;
                  public Sprite rightStickPress;

                  public Sprite a;
                  public Sprite b;
                  public Sprite c;
                  public Sprite d;
                  public Sprite e;
                  public Sprite f;
                  public Sprite g;
                  public Sprite h;
                  public Sprite i;
                  public Sprite j;
                  public Sprite k;
                  public Sprite l;
                  public Sprite m;
                  public Sprite n;
                  public Sprite o;
                  public Sprite p;
                  public Sprite q;
                  public Sprite r;
                  public Sprite s;
                  public Sprite t;
                  public Sprite u;
                  public Sprite v;
                  public Sprite w;
                  public Sprite x;
                  public Sprite y;
                  public Sprite z;

                  public Sprite leftMouseButton;
                  public Sprite middleMouseButton;
                  public Sprite rightMouseButton;

                  public Sprite upArrow;
                  public Sprite downArrow;
                  public Sprite leftArrow;
                  public Sprite rightArrow;

                  public Sprite GetSprite (string controlPath)
                  {
                        switch (controlPath)
                        {
                              case "buttonSouth":
                                    return buttonSouth;
                              case "buttonNorth":
                                    return buttonNorth;
                              case "buttonEast":
                                    return buttonEast;
                              case "buttonWest":
                                    return buttonWest;
                              case "start":
                                    return startButton;
                              case "select":
                                    return selectButton;
                              case "leftTrigger":
                                    return leftTrigger;
                              case "rightTrigger":
                                    return rightTrigger;
                              case "leftShoulder":
                                    return leftShoulder;
                              case "rightShoulder":
                                    return rightShoulder;
                              case "dpad":
                                    return dpad;
                              case "dpad/up":
                                    return dpadUp;
                              case "dpad/down":
                                    return dpadDown;
                              case "dpad/left":
                                    return dpadLeft;
                              case "dpad/right":
                                    return dpadRight;
                              case "leftStick":
                                    return leftStick;
                              case "rightStick":
                                    return rightStick;
                              case "leftStickPress":
                                    return leftStickPress;
                              case "rightStickPress":
                                    return rightStickPress;

                              case "A":
                                    return a;
                              case "B":
                                    return b;
                              case "C":
                                    return c;
                              case "D":
                                    return d;
                              case "E":
                                    return e;
                              case "F":
                                    return f;
                              case "G":
                                    return g;
                              case "H":
                                    return h;
                              case "I":
                                    return i;
                              case "J":
                                    return j;
                              case "K":
                                    return k;
                              case "L":
                                    return l;
                              case "M":
                                    return m;
                              case "N":
                                    return n;
                              case "O":
                                    return o;
                              case "P":
                                    return p;
                              case "Q":
                                    return q;
                              case "R":
                                    return r;
                              case "S":
                                    return s;
                              case "T":
                                    return t;
                              case "U":
                                    return u;
                              case "V":
                                    return v;
                              case "W":
                                    return w;
                              case "X":
                                    return x;
                              case "Y":
                                    return y;
                              case "Z":
                                    return z;

                              case "LMB":
                                    return leftMouseButton;
                              case "MMB":
                                    return middleMouseButton;
                              case "RMB":
                                    return rightMouseButton;

                              case "UpArrow":
                                    return upArrow;
                              case "DownArrow":
                                    return downArrow;
                              case "LeftArrow":
                                    return leftArrow;
                              case "RightArrow":
                                    return rightArrow;
                        }
                        return null;
                  }
            }

      }

}