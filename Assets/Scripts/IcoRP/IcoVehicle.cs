using UnityEngine;

namespace IcoRP
{
    // Self-contained arcade driving prototype: no GTA files or San Andreas assets.
    [RequireComponent(typeof(Rigidbody))]
    public class IcoVehicle : MonoBehaviour
    {
        public float acceleration = 18f;
        public float reverseAcceleration = 18f;
        public float steeringDegrees = 85f;
        public float maxSpeed = 23f;
        public float dragOnGround = .7f;
        public Transform seat;
        [HideInInspector] public IcoThirdPersonPlayer driver;
        Rigidbody rb;
        float driveInput;
        float steerInput;
        float reverseLogTimer;
        void Awake()
        {
            rb=GetComponent<Rigidbody>();
            rb.mass=1250f;
            rb.interpolation=RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode=CollisionDetectionMode.ContinuousDynamic;
            rb.centerOfMass=new Vector3(0,-.45f,0);
        }
        void Update()
        {
            driveInput = driver ? (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) ? -1f : (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) ? 1f : Input.GetAxisRaw("Vertical"))) : 0f;
            steerInput = driver ? Input.GetAxisRaw("Horizontal") : 0f;
        }
        void FixedUpdate()
        {
            if (!driver) return;
            float throttle=driveInput;
            float steer=steerInput;
            // Explicit signed target velocity: negative throttle reliably drives in reverse.
            // Preserve gravity and vertical collision response.
            float speed=Vector3.Dot(rb.linearVelocity,transform.forward);
            if (throttle < -.5f && Time.time >= reverseLogTimer)
            {
                reverseLogTimer=Time.time+1f;
                Debug.Log($"Ico RP RE: entrada={throttle}, velocidade={speed:F2}, kinematic={rb.isKinematic}, constraints={rb.constraints}, posicao={transform.position}");
            }
            float targetSpeed=throttle>0f ? maxSpeed : (throttle<0f ? -maxSpeed*.35f : speed);
            if(Mathf.Abs(throttle)>.01f)
            {
                float rate=throttle<0f ? reverseAcceleration : acceleration;
                float nextSpeed=Mathf.MoveTowards(speed,targetSpeed,rate*Time.fixedDeltaTime);
                Vector3 velocity=rb.linearVelocity;
                Vector3 planar=Vector3.ProjectOnPlane(velocity,Vector3.up);
                Vector3 lateral=planar-transform.forward*speed;
                rb.linearVelocity=transform.forward*nextSpeed+lateral*.96f+Vector3.up*velocity.y;
                speed=nextSpeed;
            }
            float steering=steer*steeringDegrees*Mathf.Clamp01(Mathf.Abs(speed)/4f)*Mathf.Sign(Mathf.Abs(speed)>.15f?speed:throttle);
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
            follow=Object.FindFirstObjectByType<IcoFollowCamera>();
        }
        void Update()
        {
            if(!Input.GetKeyDown(KeyCode.E))return;
            if(!follow) follow=Object.FindFirstObjectByType<IcoFollowCamera>();
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
            else Debug.Log("Icó RP: aproxime-se a menos de "+enterDistance+" metros de um carro.");
        }
        void EnterVehicle(IcoVehicle car)
        {
            driving=car;
            var door=car.GetComponent<IcoCarDoor>();
            if(door) { door.SetOpen(true); StartCoroutine(CloseDoorAfterDelay(door)); }
            car.SetDriver(player);
            player.enabled=false;
            controller.enabled=false;
            foreach(var r in GetComponentsInChildren<Renderer>())r.enabled=false;
            transform.SetParent(car.transform);
            transform.localPosition=car.seat?car.seat.localPosition:Vector3.up*.9f;
            if(follow) { follow.target=car.transform; follow.enabled=true; }
        }
        System.Collections.IEnumerator CloseDoorAfterDelay(IcoCarDoor door)
        {
            yield return new WaitForSeconds(.85f);
            if(door)door.SetOpen(false);
        }
        void ExitVehicle()
        {
            var car=driving;
            var door=car.GetComponent<IcoCarDoor>();
            if(door) { door.SetOpen(true); StartCoroutine(CloseDoorAfterDelay(door)); }
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
