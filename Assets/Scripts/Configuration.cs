using UnityEngine;
using System.Collections;

public class Configuration {
	
	public class Cannon {
		public const float MIN_ANGLE = 0.0f;
		public const float MAX_ANGLE = 60.0f;
		public const float MIN_POWER = 12.0f;
		public const float MAX_POWER = 50.0f;
	}
	
	public class CannonBall {
		public const float STOP_SPEED = 0.4f;
		public const float STOP_HEIGHT = -0.2f;
		public const float STOP_MINHEIGHT = -9.5f;
	}
	
	public class PerpsCamera {
		public static float BOTTOM_LIMIT = -5.0f;
	}
	
	public class OrthoCamera {
		public static Vector3 INITIAL_TARGET_POS = new Vector3 ( 2.0f, 2.0f, 0.0f );
		public const float INITIAL_SIZE = 25.0f;
		public const float MIN_SIZE = 10.0f;
		public const float MAX_SIZE = 100.0f;
	}

    public class Objects
    {
        public const float GROUND_LEFT = 30.0f;
        public const float GROUND_MIN = -0.1f;
        public const float GROUND_MAX = 2.0f;
        public const float SKY_MIN = 5.0f;
        public const float SKY_MAX = float.MaxValue;
    }
	
}
