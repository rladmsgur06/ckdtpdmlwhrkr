using System.Collections.Generic;
using UnityEngine;

namespace TwoBitMachines.FlareEngine
{
        public class GridItem : MonoBehaviour
        {
                [SerializeField] public bool gridElement = true;
                [SerializeField] public bool mustPay = false;
                [SerializeField] public bool save = false;
                [SerializeField] public float cost = 10f;
                [SerializeField] public string ID;

                [SerializeField] public string selectWE;
                [SerializeField] public string onFocusWE;
                [SerializeField] public string onOutOfFocusWE;
                [SerializeField] public string hasBeenUsedWE;
                [SerializeField] public string purchaseFailedWE;
                [SerializeField] public string cantPurchaseWE;
                [SerializeField] public string purchaseSuccessWE;
                [SerializeField] public string onHasBeenPurchasedWE;

                [SerializeField] public UnityEventEffect onSelect = new UnityEventEffect();
                [SerializeField] public UnityEventEffect onFocus = new UnityEventEffect();
                [SerializeField] public UnityEventEffect onOutOfFocus = new UnityEventEffect();
                [SerializeField] public UnityEventEffect purchaseFailed = new UnityEventEffect();
                [SerializeField] public UnityEventEffect purchaseSuccess = new UnityEventEffect();
                [SerializeField] public UnityEventEffect cantPurchase = new UnityEventEffect();
                [SerializeField] public UnityEventEffect onHasBeenPurchased = new UnityEventEffect();

                [System.NonSerialized] public bool rememberPurchase;
                [System.NonSerialized] private AI.Grid parent;
                [System.NonSerialized] private List<Transform> children = new List<Transform>();

                #region ▀▄▀▄▀▄ Editor Variables ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField, HideInInspector] private bool eventsFoldOut;
                [SerializeField, HideInInspector] private bool selectFoldOut;
                [SerializeField, HideInInspector] private bool onFocusFoldOut;
                [SerializeField, HideInInspector] private bool cantPurchaseFoldOut;
                [SerializeField, HideInInspector] private bool onOutOfFocusFoldOut;
                [SerializeField, HideInInspector] private bool purchaseFailedFoldOut;
                [SerializeField, HideInInspector] private bool purchaseSuccessFoldOut;
                [SerializeField, HideInInspector] private bool onHasBeenPurchasedFoldOut;
#pragma warning restore 0414
#endif
                #endregion

                public void Initialize (AI.Grid parentRef)
                {
                        parent = parentRef;
                        WorldManager.get.worldResetAll -= ResetAll;
                        WorldManager.get.worldResetAll += ResetAll;

                        for (int i = 0; i < transform.childCount; i++)
                        {
                                Transform child = transform.GetChild(i);
                                GridItem gridItemChild = child.GetComponent<GridItem>();
                                if (gridItemChild != null)
                                {
                                        gridItemChild.Initialize(parentRef);
                                }
                                else if (gridElement)
                                {
                                        child.SetParent(parentRef.transform.parent);
                                        children.Add(child);
                                        child.gameObject.SetActive(true);
                                }
                        }

                        if (gridElement)
                        {
                                gameObject.SetActive(true);
                        }
                        if (save && parentRef.ItemIsPurchased(ID))
                        {
                                rememberPurchase = true;
                                onHasBeenPurchased.Invoke(ImpactPacket.impact.Set(onHasBeenPurchasedWE, transform, null, transform.position, null, Vector2.zero, 0, 0));
                        }
                }

                public void ResetAll ()
                {
                        if (gameObject.activeInHierarchy)
                        {
                                onOutOfFocus.Invoke(ImpactPacket.impact.Set(onOutOfFocusWE, transform, null, transform.position, null, Vector2.zero, 0, 0));
                        }
                        if (gridElement)
                        {
                                if (gameObject.activeInHierarchy)
                                {
                                        gameObject.SetActive(false);
                                }
                                for (int i = 0; i < children.Count; i++)
                                {
                                        if (children[i].gameObject.activeInHierarchy)
                                        {
                                                children[i].SetParent(transform);
                                                children[i].gameObject.SetActive(false);
                                        }
                                }
                        }
                }

                public void OnSelect ()
                {
                        if (mustPay && rememberPurchase)
                        {
                                cantPurchase.Invoke(ImpactPacket.impact.Set(cantPurchaseWE, transform, null, transform.position, null, Vector2.zero, 0, 0));
                                return;
                        }
                        if (mustPay && parent != null && parent.payment != null)
                        {
                                float money = parent.payment.GetValue();
                                if (money < cost || parent.payment.cantIncrement)
                                {
                                        purchaseFailed.Invoke(ImpactPacket.impact.Set(purchaseFailedWE, transform, null, transform.position, null, Vector2.zero, 0, 0));
                                        return;
                                }
                                else
                                {
                                        if (parent.useTempValue)
                                        {
                                                parent.payment.IncreaseTempValue(-cost);
                                        }
                                        else
                                        {
                                                parent.payment.Increment(-cost);
                                        }
                                        if (save)
                                        {
                                                parent.SaveItem(ID);
                                                rememberPurchase = true;
                                        }
                                        purchaseSuccess.Invoke(ImpactPacket.impact.Set(purchaseSuccessWE, transform, null, transform.position, null, Vector2.zero, 0, 0));
                                }

                        }

                        onSelect.Invoke(ImpactPacket.impact.Set(selectWE, transform, null, transform.position, null, Vector2.zero, 0, 0));
                }

                public void OnFocus ()
                {
                        onFocus.Invoke(ImpactPacket.impact.Set(onFocusWE, transform, null, transform.position, null, Vector2.zero, 0, 0));
                }

                public void OnOutOfFocus ()
                {
                        onOutOfFocus.Invoke(ImpactPacket.impact.Set(onOutOfFocusWE, transform, null, transform.position, null, Vector2.zero, 0, 0));
                }
                public void OnMouseDown ()
                {
                        if (parent != null)
                        {
                                parent.SelectThisGridItem(transform);
                        }
                }
        }
}
