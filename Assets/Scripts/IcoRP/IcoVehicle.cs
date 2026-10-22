using UnityEngine;

namespace IcoRP
{
    // Self-contained arcade driving prototype: no GTA files or San Andreas assets.
    [RequireComponent(typeof(Rigidbody))]
    public class IcoVehicle : MonoBehaviour
    {
        public float acceleration = 18f;
        public float reverseAcceleration = 9f;
        public float steeringDegrees = 85f;
        public float maxSpeed = 23f;
        public float dragOnGround = .7f;
        public Transform seat;
        [HideInInspector] public IcoThirdPersonPlayer driver;
        Rigidbody rb;
        void Awake()
        {
            rb=GetComponent<Rigidbody>();
            rb.mass=1250f;
            rb.interpolation=RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode=CollisionDetectionMode.ContinuousDynamic;
            rb.centerOfMass=new Vector3(0,-.45f,0);
        }
        void FixedUpdate()
        {
            if (!driver) return;
            float throttle=Input.GetAxisRaw("Vertical");
            float steer=Input.GetAxisRaw("Horizontal");
            float speed=Vector3.Dot(rb.linearVelocity,transform.forward);
            if ((throttle>0 && speed<maxSpeed)||(throttle<0 && speed>-maxSpeed*.35f))
                rb.AddForce(transform.forward*throttle*(throttle>0?acceleration:reverseAcceleration)*rb.mass,ForceMode.Force);
            float steering=steer*steeringDegrees*Mathf.Clamp01(Mathf.Abs(speed)/4f)*Mathf.Sign(speed);
            rb.MoveRotation(rb.rotation*Quaternion.Euler(0,steering*Time.fixedDeltaTime,0));
        }
        public void SetDriver(IcoThirdPersonPlayer player) { driver=player; }
    }

    public class IcoVehicleInteraction : MonoBehaviour
    {
        public float enterDistance=4f;
        IcoThirdPersonPlayer player;
        IcoVehicle driving;
        CharacterController controller;
        IcoFollowCamera follow;
        void Awake()
        {
            player=GetComponent<IcoThirdPersonPlayer>();
            controller=GetComponent<CharacterController>();
            var camera=Camera.main;
            if(camera)follow=camera.GetComponent<IcoFollowCamera>();
        }
        void Update()
        {
            if(!Input.GetKeyDown(KeyCode.E))return;
            if(driving){ ExitVehicle();return; }
            IcoVehicle nearest=null;
            float best=enterDistance*enterDistance;
            foreach(var car in Object.FindObjectsByType<IcoVehicle>(FindObjectsSortMode.None))
            {
                if(car.driver)continue;
                float d=(car.transform.position-transform.position).sqrMagnitude;
                if(d<best){best=d;nearest=car;}
            }
            if(nearest)EnterVehicle(nearest);
        }
        void EnterVehicle(IcoVehicle car)
        {
            driving=car;
            car.SetDriver(player);
            player.enabled=false;
            controller.enabled=false;
            foreach(var r in GetComponentsInChildren<Renderer>())r.enabled=false;
            transform.SetParent(car.transform);
            transform.localPosition=car.seat?car.seat.localPosition:Vector3.up*.9f;
            if(follow)follow.target=car.transform;
        }
        void ExitVehicle()
        {
            var car=driving;
            transform.SetParent(null);
            transform.position=car.transform.position+car.transform.right*2.6f+Vector3.up*.7f;
            foreach(var r in GetComponentsInChildren<Renderer>())r.enabled=true;
            controller.enabled=true;
            player.enabled=true;
            car.SetDriver(null);
            driving=null;
            if(follow)follow.target=transform;
        }
    }
}
