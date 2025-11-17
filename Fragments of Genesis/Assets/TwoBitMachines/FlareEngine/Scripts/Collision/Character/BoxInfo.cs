using UnityEngine;

namespace TwoBitMachines.FlareEngine
{
        [System.Serializable]
        public class BoxInfo
        {
                [SerializeField] public Vector2Int rays = new Vector2Int(3, 2);

                [System.NonSerialized] public BoxCollider2D collider;
                [System.NonSerialized] public WorldCollision world;
                [System.NonSerialized] public Vector2 skin = new Vector2(0.00025f, 0.01f); // This makes an angle of 88.57, thus we'll make max slope angle 88 degrees

                [System.NonSerialized] public Vector2 up;
                [System.NonSerialized] public Vector2 down;
                [System.NonSerialized] public Vector2 size;
                [System.NonSerialized] public Vector2 right;
                [System.NonSerialized] public Vector2 boxSize;
                [System.NonSerialized] public Vector2 boxOffset;
                [System.NonSerialized] public Vector2 spacing;
                [System.NonSerialized] public Vector2 topLeft;
                [System.NonSerialized] public Vector2 topRight;
                [System.NonSerialized] public Vector2 bottomLeft;
                [System.NonSerialized] public Vector2 bottomRight;

                [System.NonSerialized] public float sizeY;
                [System.NonSerialized] public float sizeX;

                public static Vector2 topLeftCorner;
                public static Vector2 topRightCorner;
                public static Vector2 bottomLeftCorner;
                public static Vector2 bottomRightCorner;
                public static Vector2[] cornerA = new Vector2[4];
                public static Vector2[] cornerB = new Vector2[4];

                public int directionX => (int) Mathf.Sign(right.x);
                public float top => topRight.y + (up.y * skin.y);
                public float bottom => bottomRight.y - (up.y * skin.y);
                public float angle => Compute.Round(collider.transform.eulerAngles.z, 0.001f);
                public float diagonalLength => ((topLeft + skinY) - (bottomRight - skinY)).magnitude;

                public Vector2 skinX => right * skin.x;
                public Vector2 skinY => up * skin.y;
                public Vector2 height => up * sizeY;
                public Vector2 cornerTopLeft => topLeft + skinY;
                public Vector2 topCenter => topLeft + skinY + right * (size.x * 0.5f);
                public Vector2 bottomCenter => bottomLeft - skinY + right * (size.x * 0.5f);
                public Vector2 center => bottomLeft + (up * size.y * 0.5f) + right * (size.x * 0.5f);

                public Vector2 ToWorldSpace (Vector2 velocity)
                {
                        return velocity.x * right + velocity.y * up;
                }

                public void Initialize (WorldCollision worldRef, BoxCollider2D colliderRef)
                {
                        world = worldRef;

                        if (colliderRef != null)
                        {
                                collider = colliderRef;
                                boxSize = collider.size;
                                boxOffset = collider.offset;
                                CalculateCharacterSize();
                                Update();
                        }
                }

                public void CalculateCharacterSize ()
                {
                        sizeX = collider.size.x * collider.transform.localScale.x;
                        sizeY = collider.size.y * collider.transform.localScale.y;
                        size.x = sizeX;
                        size.y = sizeY - skin.y * 2f;
                        spacing = new Vector2(size.x / ((float) rays.y - 1f), size.y / ((float) rays.x - 1f));
                }

                public void ChangeColliderHeight (float newHeight, float offset = 0)
                {
                        Vector2 oldSize = collider.size;
                        collider.size = new Vector2(oldSize.x, newHeight);
                        collider.offset = new Vector2(boxOffset.x, (newHeight * 0.5f) + offset);
                        CalculateCharacterSize();
                        Update();
                }

                public void ColliderReset ()
                {
                        collider.size = boxSize;
                        collider.offset = boxOffset;
                        CalculateCharacterSize();
                        Update();
                }

                public void ColliderBasicReset ()
                {
                        collider.size = boxSize;
                        collider.offset = boxOffset;
                }

                public void Update ()
                {
                        UpdateAngle();
                        UpdateCorners();
                }

                public void CornerUpdateUnmodified ()
                {
                        UpdateAngle();
                        GetColliderCorners(collider);
                        topLeft = topLeftCorner;
                        topRight = topRightCorner;
                        bottomLeft = bottomLeftCorner;
                        bottomRight = bottomRightCorner;
                }

                public void UpdateCorners ()
                {
                        GetColliderCorners(collider);
                        Vector2 offsetY = up * skin.y;
                        topLeft = topLeftCorner - offsetY;
                        topRight = topRightCorner - offsetY;
                        bottomLeft = bottomLeftCorner + offsetY;
                        bottomRight = bottomRightCorner + offsetY;
                }

                public void UpdateCornersManually (Vector2 velocity)
                {
                        Vector2 update = ToWorldSpace(velocity);
                        topLeft += update;
                        topRight += update;
                        bottomLeft += update;
                        bottomRight += update;
                }

                public void UpdateAngle ()
                {
                        Quaternion identity = collider.transform.rotation;
                        up = identity * Vector2.up;
                        down = identity * Vector2.down;
                        right = identity * Vector2.right;
                }

                public void EnableCollider ()
                {
                        if (collider != null)
                        {
                                collider.enabled = true;
                        }
                }

                public Vector2 TopExactCorner (float direction)
                {
                        return direction < 0 ? topLeft + skinY : topRight + skinY;
                }

                public Vector2 BottomCorner (float direction)
                {
                        return direction < 0 ? bottomLeft : bottomRight;
                }

                public Vector2 BottomExactCorner (float direction)
                {
                        return direction < 0 ? bottomLeft - skinY : bottomRight - skinY;
                }

                public Vector2 MoveToBottomPoint (Vector2 moveToPoint, float cornerDirection)
                {
                        Vector2 velocity = moveToPoint - BottomExactCorner(cornerDirection);
                        if (velocity == Vector2.zero)
                                return Vector2.zero;
                        float magnitude = velocity.magnitude;
                        Vector2 direction = velocity / magnitude;
                        float angle = Vector2.Angle(down, direction);
                        return float.IsNaN(angle) ? Vector2.zero : Compute.RotateVector(Vector2.down, angle * down.CrossSign(direction)) * magnitude;
                }

                public void UpdateLeftCorner ()
                {
                        UpdateAngle();

                        Transform transform = collider.transform;
                        Vector2 worldPosition = transform.TransformPoint(0, 0, 0);
                        Vector2 halfSize = new Vector2(collider.size.x * transform.localScale.x * 0.5f, collider.size.y * transform.localScale.y * 0.5f);
                        Vector2 boxOffset = new Vector2(collider.offset.x * transform.localScale.x, collider.offset.y * transform.localScale.y);
                        bottomLeftCorner = new Vector2(-halfSize.x, -halfSize.y) + boxOffset;
                        float cos = Mathf.Cos(Mathf.Deg2Rad * transform.eulerAngles.z);
                        float sin = Mathf.Sin(Mathf.Deg2Rad * transform.eulerAngles.z);
                        bottomLeftCorner = Compute.RotateVector(bottomLeftCorner, cos, sin);
                        bottomLeft = worldPosition + bottomLeftCorner;
                }

                public static void GetColliderCorners (BoxCollider2D box)
                {
                        if (box == null)
                                return;

                        Transform transform = box.transform;

                        // The collider's centre point in the world
                        Vector2 worldPosition = transform.TransformPoint(0, 0, 0);

                        // The collider's local width and height, accounting for scale, divided by 2
                        Vector2 halfSize = new Vector2(box.size.x * transform.localScale.x * 0.5f, box.size.y * transform.localScale.y * 0.5f);
                        Vector2 boxOffset = new Vector2(box.offset.x * transform.localScale.x, box.offset.y * transform.localScale.y);

                        topLeftCorner = new Vector2(-halfSize.x, halfSize.y) + boxOffset;
                        topRightCorner = new Vector2(halfSize.x, halfSize.y) + boxOffset;
                        bottomLeftCorner = new Vector2(-halfSize.x, -halfSize.y) + boxOffset;
                        bottomRightCorner = new Vector2(halfSize.x, -halfSize.y) + boxOffset;

                        // STEP 2: ROTATE CORNERS
                        // Rotate those 4 corners around the centre of the collider to match its transform.rotation
                        if (transform.eulerAngles.z != 0)
                        {
                                float cos = Mathf.Cos(Mathf.Deg2Rad * transform.eulerAngles.z);
                                float sin = Mathf.Sin(Mathf.Deg2Rad * transform.eulerAngles.z);
                                topLeftCorner = Compute.RotateVector(topLeftCorner, cos, sin);
                                topRightCorner = Compute.RotateVector(topRightCorner, cos, sin);
                                bottomLeftCorner = Compute.RotateVector(bottomLeftCorner, cos, sin);
                                bottomRightCorner = Compute.RotateVector(bottomRightCorner, cos, sin);
                        }

                        // STEP 3: FIND WORLD POSITION OF CORNERS
                        // Add the 4 rotated corners above to our centre position in WORLD space - and we're done!
                        topLeftCorner = worldPosition + topLeftCorner;
                        topRightCorner = worldPosition + topRightCorner;
                        bottomLeftCorner = worldPosition + bottomLeftCorner;
                        bottomRightCorner = worldPosition + bottomRightCorner;
                }

                public static void GetEdgeCorners (EdgeCollider2D edge)
                {
                        if (edge == null)
                                return;
                        Transform transform = edge.transform;

                        // The collider's centre point in the world
                        Vector2 worldPosition = transform.TransformPoint(0, 0, 0);
                        Vector2 boxOffset = new Vector2(edge.offset.x * transform.localScale.x, edge.offset.y * transform.localScale.y);
                        topLeftCorner = edge.points[0] + boxOffset;
                        topRightCorner = edge.points[1] + boxOffset;

                        // STEP 2: ROTATE CORNERS
                        if (transform.eulerAngles.z != 0)
                        {
                                float cos = Mathf.Cos(Mathf.Deg2Rad * transform.eulerAngles.z);
                                float sin = Mathf.Sin(Mathf.Deg2Rad * transform.eulerAngles.z);
                                topLeftCorner = Compute.RotateVector(topLeftCorner, cos, sin);
                                topRightCorner = Compute.RotateVector(topRightCorner, cos, sin);
                        }

                        // STEP 3: FIND WORLD POSITION OF CORNERS
                        topLeftCorner = worldPosition + topLeftCorner;
                        topRightCorner = worldPosition + topRightCorner;

                }

                public bool BoxTouchingAABB (BoxCollider2D collider, float bottomOffset = 0.01f) // unrotated
                {
                        GetColliderCorners(collider);
                        float left = bottomLeftCorner.x;
                        float right = bottomRightCorner.x;
                        float top = topLeftCorner.y;
                        float bottom = bottomRightCorner.y - bottomOffset;

                        GetColliderCorners(this.collider);
                        cornerB[0] = bottomLeftCorner;
                        cornerB[1] = bottomRightCorner;
                        cornerB[2] = topLeftCorner;
                        cornerB[3] = topRightCorner;

                        for (int i = 0; i < 4; i++)
                        {
                                Vector2 p = cornerB[i];
                                if (p.x > left && p.x < right && p.y < top && p.y > bottom)
                                {
                                        return true;
                                }
                        }
                        return false;
                }

                public static bool BoxTouching (BoxCollider2D collider, LayerMask layer, float offsetX = 0, float offsetY = 0) // unrotated
                {
                        GetColliderCorners(collider);
                        cornerA[0] = bottomLeftCorner - Vector2.right * offsetX + Vector2.up * offsetY;
                        cornerA[1] = topLeftCorner - Vector2.right * offsetX - Vector2.up * offsetY;
                        cornerA[2] = topRightCorner + Vector2.right * offsetX - Vector2.up * offsetY;
                        cornerA[3] = bottomRightCorner + Vector2.right * offsetX + Vector2.up * offsetY;

                        // Draw.Circle (cornerA[0], 0.05f, Color.red);
                        // Draw.Circle (cornerA[1], 0.05f, Color.red);
                        // Draw.Circle (cornerA[2], 0.05f, Color.red);
                        // Draw.Circle (cornerA[3], 0.05f, Color.red);

                        for (int i = 0; i < 4; i++)
                        {
                                Collider2D b = Physics2D.OverlapPoint(cornerA[i], layer); //) return true;
                                if (b != null && b != collider)
                                {
                                        return true;
                                }
                        }
                        return false;
                }

        }

}
