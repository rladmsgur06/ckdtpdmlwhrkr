using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace TwoBitMachines.Safire2DCamera.TestMode
{
	public class TestMode : MonoBehaviour
	{
		[SerializeField] public Player player = new Player ( );
		[SerializeField] public PlaySprite anim = new PlaySprite ( );
		void Awake ( )
		{
			anim.Initialize ( );
			player.Initialize (anim);
		}

		void Update ( )
		{
			player.Update ( );
			anim.Update ( );
		}
	}

	[System.Serializable]
	public class Player
	{
		[SerializeField] public float moveSpeed = 25;
		[SerializeField] public Transform transform
		;
		[System.NonSerialized] private WorldCollider world;
		[System.NonSerialized] private LayerMask worldCollision;
		[System.NonSerialized] private SpriteRenderer render;
		[System.NonSerialized] private BoxCollider2D box2D;
		[System.NonSerialized] private Vector2 velocity;
		[System.NonSerialized] private Inputs button;

		[System.NonSerialized] private float jumpHeight = 7f;
		[System.NonSerialized] private float timeInAir = 0.5f;
		[System.NonSerialized] private float minJumpHeight = 1f;
		[System.NonSerialized] private float gravity;
		[System.NonSerialized] private float jumpForce;
		[System.NonSerialized] private float minJumpForce;
		[System.NonSerialized] private bool pointLeft;
		[System.NonSerialized] private bool pointPrevious;
		[System.NonSerialized] private float speed = 0;

		PlaySprite anim;

		public void Initialize (PlaySprite playSprite)
		{
			worldCollision = LayerMask.GetMask ("World");
			gravity = -(2 * jumpHeight) / Mathf.Pow (timeInAir, 2);
			jumpForce = Mathf.Abs (gravity) * timeInAir;
			minJumpForce = Mathf.Sqrt (2 * Mathf.Abs (gravity) * minJumpHeight);
			render = transform.gameObject.GetComponent<SpriteRenderer> ( );
			box2D = transform.gameObject.GetComponent<BoxCollider2D> ( );
			world = new WorldCollider (box2D, transform, worldCollision);
			anim = playSprite;
			button = new Inputs ( );
		}

		public void Update ( )
		{
			button.Update ( );
			speed = Input.GetKey (KeyCode.P) ? moveSpeed * 2 : moveSpeed;
			Execute ( );
		}

		private void Execute ( )
		{

			Vector2 input = button.axis;
			velocity.x = input.x * speed;
			velocity.y += gravity * Time.deltaTime;
			Direction (input.x);
			if (world.ground || world.above)
				velocity.y = -0.1f;

			if (button.jumpHold && world.ground)
			{
				button.jumping = true; // in air
				velocity.y = jumpForce;
			}
			if (button.jumpReleased)
			{
				if (velocity.y > minJumpForce)
					velocity.y = minJumpForce;
			}
			Direction (velocity.x);
			world.Check (velocity * Time.deltaTime);

			if (world.ground)
			{
				if (velocity.x == 0) anim.Set ("Stand");
				else anim.Set ("Run");
			}
			else
			{
				if (velocity.y > 0) anim.Set ("Jump");
				else anim.Set ("Fall");
			}
			render.flipX = pointLeft;

		}

		private void Direction (float left)
		{
			pointLeft = (left == 0) ? pointLeft : !(left > 0);
			if (pointLeft != pointPrevious) pointPrevious = pointLeft;
		}
	}

	public class Inputs
	{
		[System.NonSerialized] public LayerMask wallMask;
		[System.NonSerialized] public Vector2 axis;
		[System.NonSerialized] public bool jumpHold;
		[System.NonSerialized] public bool jumpPressed;
		[System.NonSerialized] public bool jumpReleased;
		[System.NonSerialized] public bool onWall;
		[System.NonSerialized] public bool jumping;
		[System.NonSerialized] public bool left, right;
		[System.NonSerialized] public bool released;
		[System.NonSerialized] private float timer;
		[System.NonSerialized] private bool pressReleased;

		public void Update ( )
		{
			jumping = false;
			Keyboard ( );
			if (released && pressReleased)
			{
				pressReleased = false;
				return;
			}
		}

		public void Keyboard ( )
		{
			axis = new Vector2 (Input.GetAxisRaw ("Horizontal"), 0);
			jumpHold = Input.GetKey (KeyCode.Space);
			jumpReleased = Input.GetKeyUp (KeyCode.Space);
			jumpPressed = Input.GetKeyDown (KeyCode.Space);

			released = Input.GetKeyUp (KeyCode.J);
			left = Input.GetKey (KeyCode.A);
			right = Input.GetKey (KeyCode.D);
		}
	}

	public class WorldCollider
	{
		public const float skinWidth = 0.015f;

		[System.NonSerialized] private Transform transform;
		[System.NonSerialized] private LayerMask collisionMask;
		[System.NonSerialized] public int horizontalRays = 3;
		[System.NonSerialized] public int verticalRays = 2;
		[System.NonSerialized] public BoxCollider2D collider;
		[System.NonSerialized] public Bounds player;
		[System.NonSerialized] public float horizontalSpacing;
		[System.NonSerialized] public float verticalSpacing;
		[System.NonSerialized] public float horizontalSpacingFlipped;
		[System.NonSerialized] public float verticalSpacingFlipped;
		[System.NonSerialized] public Vector2 topLeft, topRight;
		[System.NonSerialized] public Vector2 bottomLeft, bottomRight;
		[System.NonSerialized] public bool above, ground, left, right, movingPlatformY, movingPlatformX, nearWall, mid;
		[System.NonSerialized] public Transform movingPlatTransform;

		public void Update (BoxCollider2D collider)
		{
			UnityEngine.Bounds bounds = collider.bounds;
			bounds.Expand (skinWidth * -2);
			bottomLeft = new Vector2 (bounds.min.x, bounds.min.y);
			bottomRight = new Vector2 (bounds.max.x, bounds.min.y);
			topLeft = new Vector2 (bounds.min.x, bounds.max.y);
			topRight = new Vector2 (bounds.max.x, bounds.max.y);
			above = ground = left = right = mid = movingPlatformY = movingPlatformX = nearWall = false;
		}

		public void CalculateRaysSpacing ( )
		{
			UnityEngine.Bounds bounds = collider.bounds;
			bounds.Expand (skinWidth * -2);
			horizontalRays = Mathf.Clamp (horizontalRays, 2, int.MaxValue);
			verticalRays = Mathf.Clamp (verticalRays, 2, int.MaxValue);
			horizontalSpacing = bounds.size.y / (horizontalRays - 1);
			verticalSpacing = bounds.size.x / (verticalRays - 1);
		}

		public WorldCollider (BoxCollider2D collider, Transform transform, LayerMask collisionMask)
		{
			this.collider = collider;
			this.transform = transform;
			this.collisionMask = collisionMask;
			CalculateRaysSpacing ( );
		}

		public void Check (Vector2 velocity)
		{
			Update (collider);
			HorizontalCollision (ref velocity);
			VerticalCollision (ref velocity);
			transform.Translate (velocity);
		}

		public void HorizontalCollision (ref Vector2 velocity)
		{
			if (velocity.x == 0)
				return;

			float directionX = Mathf.Sign (velocity.x);
			float rayLength = Mathf.Abs (velocity.x) + skinWidth;
			for (int i = 0; i < horizontalRays; i++)
			{
				Vector2 rayOrigin = directionX == -1 ? bottomLeft : bottomRight;
				rayOrigin += Vector2.up * (horizontalSpacing * i);
				RaycastHit2D wall = Physics2D.Raycast (rayOrigin, Vector2.right * directionX, rayLength, collisionMask);
				if (wall)
				{
					if (wall.distance == 0)
						continue;
					velocity.x = (wall.distance - skinWidth) * directionX;
					rayLength = wall.distance;
					left = directionX == -1;
					right = directionX == 1;
					nearWall = (left || right);
					if (i != 2) mid = true;
				}
			}
		}

		public void VerticalCollision (ref Vector2 velocity)
		{
			if (velocity.y == 0)
				return;
			float directionY = Mathf.Sign (velocity.y);
			float rayLength = Mathf.Abs (velocity.y) + skinWidth;
			for (int i = 0; i < verticalRays; i++)
			{
				Vector2 rayOrigin = directionY == -1 ? bottomLeft : topLeft;
				rayOrigin += Vector2.right * (verticalSpacing * i + velocity.x);
				RaycastHit2D wall = Physics2D.Raycast (rayOrigin, Vector2.up * directionY, rayLength, collisionMask);
				if (wall)
				{
					velocity.y = (wall.distance - skinWidth) * directionY;
					rayLength = wall.distance;
					ground = directionY == -1;
					above = directionY == 1;
				}
			}
		}
	}

	[System.Serializable]
	public class PlaySprite
	{
		[SerializeField] public Sprites[] sprites;
		[SerializeField] public Transform transform;

		public bool pause { get; set; }
		public Sprites current { get; set; }
		public SpriteRenderer render { get; set; }
		private Dictionary<string, Sprites> sprite = new Dictionary<string, Sprites> ( );

		public void Initialize ( )
		{
			render = transform.gameObject.GetComponent<SpriteRenderer> ( );
			InitializeSprites ( );
			SetSpriteReference ( );
			if (sprites.Length > 0)
				current = sprites[0];
		}

		public void Set (string spriteName)
		{
			if (current.name == spriteName)
				return;
			Reset (spriteName);
		}

		private void Reset (string name)
		{
			Sprites newSprite;
			if (sprite.TryGetValue (name, out newSprite))
			{
				current = newSprite.Reset ( );
				pause = false;
			}
		}

		public void Update ( )
		{
			if (!pause)
				current.Play ( );
		}

		public void InitializeSprites ( )
		{
			for (int i = 0; i < sprites.Length; i++)
				sprites[i].Initialize (this);
		}

		public void SetSpriteReference ( )
		{
			for (int i = 0; i < sprites.Length; i++)
				sprite.Add (sprites[i].name, sprites[i]);
		}
	}

	[System.Serializable]
	public class Sprites
	{
		[SerializeField] public string name;
		[SerializeField] public float rate;
		[SerializeField] public Sprite[] frame;

		[System.NonSerialized] private PlaySprite play;
		[System.NonSerialized] private float timer;

		public int next { get; private set; }

		public void Initialize (PlaySprite script)
		{
			play = script;
		}

		public Sprites Reset ( )
		{
			timer = next = 0;
			play.render.sprite = frame[next];
			return this;
		}

		public void Play ( )
		{
			if (Clock.Timer (ref timer, rate))
			{
				next = (++next == frame.Length) ? 0 : next;
				play.render.sprite = frame[next];
			}
		}
	}

}