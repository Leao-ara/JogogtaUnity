using UnityEngine;

namespace IcoRP
{
    // Self-contained arcade driving prototype: no GTA files or San Andreas assets.
    // Arcade vehicle controller. No external GTA game assets required.
    [RequireComponent(typeof(Rigidbody))]
    public class IcoVehicle : MonoBehaviour
    {
        public float acceleration=12f;
        public float reverseAcceleration=18f;
        public float brakeDeceleration=22f;
        public float steeringDegrees=95f;
        public float maxSpeed=23f;
        public float maxReverseSpeed=14f;
        public float lateralGrip=7f;
        public float suspensionHeight=.65f;
        public float suspensionSpring=16000f;
        public float suspensionDamping=2100f;
        public float rolloverRecoverySeconds=3f;
        public Transform seat;
        public Transform[] suspensionPoints;
        public Light[] headlights;
        public AudioSource horn;
        [HideInInspector] public IcoThirdPersonPlayer driver;
        Rigidbody rb;
        IcoVehicleDamage damage;
        float throttle,steer;
        bool brake,lightsOn;
        float upsideDownTime;
        void Awake()
        {
            rb=GetComponent<Rigidbody>();
            rb.mass=1250f;
            rb.interpolation=RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode=CollisionDetectionMode.ContinuousDynamic;
            rb.centerOfMass=new Vector3(0,-.45f,0);
            rb.isKinematic=false;
            rb.constraints=RigidbodyConstraints.None;
            damage=GetComponent<IcoVehicleDamage>();
            if(!damage)damage=gameObject.AddComponent<IcoVehicleDamage>();
            foreach(var light in headlights)if(light)light.enabled=false;
        }
        void Update()
        {
            if(!driver){throttle=steer=0;brake=false;return;}
            // Read keys directly: reliable even if legacy Input axes are misconfigured.
            bool forward=Input.GetKey(KeyCode.W)||Input.GetKey(KeyCode.UpArrow);
            bool reverse=Input.GetKey(KeyCode.S)||Input.GetKey(KeyCode.DownArrow);
            throttle=forward==reverse?0:(forward?1:-1);
            steer=(Input.GetKey(KeyCode.D)||Input.GetKey(KeyCode.RightArrow)?1:0)
                 -(Input.GetKey(KeyCode.A)||Input.GetKey(KeyCode.LeftArrow)?1:0);
            brake=Input.GetKey(KeyCode.Space);
            if(Input.GetKeyDown(KeyCode.L))
            {
                lightsOn=!lightsOn;
                foreach(var light in headlights)if(light)light.enabled=lightsOn;
            }
            if(horn)
            {
                if(Input.GetKeyDown(KeyCode.H)&&horn.clip)horn.Play();
                if(Input.GetKeyUp(KeyCode.H))horn.Stop();
            }
            if(Input.GetKeyDown(KeyCode.R))RecoverVehicle();
            if(Input.GetKeyDown(KeyCode.F)&&damage)damage.Repair();
        }
        void FixedUpdate()
        {
            if(!driver)return;
            float dt=Time.fixedDeltaTime;
            Vector3 forward=Vector3.ProjectOnPlane(transform.forward,Vector3.up).normalized;
            if(forward.sqrMagnitude<.1f)forward=transform.forward;
            float speed=Vector3.Dot(rb.linearVelocity,forward);
            float rate=throttle<0?reverseAcceleration:acceleration;
            float target=throttle>0?maxSpeed:throttle<0?-maxReverseSpeed:0f;
            if(Mathf.Abs(throttle)>.01f)
            {
                // Brakes first when changing direction, then reverse from rest.
                float changeRate=speed*throttle<0?brakeDeceleration:rate;
                float next=Mathf.MoveTowards(speed,target,changeRate*dt);
                rb.linearVelocity+=forward*(next-speed);
                speed=next;
            }
            if(brake)
            {
                Vector3 horizontal=Vector3.ProjectOnPlane(rb.linearVelocity,Vector3.up);
                rb.linearVelocity-=Vector3.MoveTowards(Vector3.zero,horizontal,brakeDeceleration*dt);
            }
            else if(Mathf.Abs(throttle)<.01f)
            {
                rb.linearVelocity-=forward*Mathf.Sign(speed)*Mathf.Min(Mathf.Abs(speed),2.5f*dt);
            }
            Vector3 lateral=Vector3.ProjectOnPlane(rb.linearVelocity,Vector3.up)-forward*Vector3.Dot(rb.linearVelocity,forward);
            rb.AddForce(-lateral*lateralGrip,ForceMode.Acceleration);
            if(Mathf.Abs(speed)>.15f)
            {
                float turn=steer*steeringDegrees*Mathf.Clamp01(Mathf.Abs(speed)/5f)*Mathf.Sign(speed);
                rb.MoveRotation(rb.rotation*Quaternion.Euler(0,turn*dt,0));
            }
            // Optional raycast spring suspension. Leave points empty for test blocks.
            if(suspensionPoints!=null)
            foreach(var point in suspensionPoints)
            {
                if(!point)continue;
                if(Physics.Raycast(point.position,-transform.up,out RaycastHit hit,suspensionHeight,~0,QueryTriggerInteraction.Ignore)
                    && !hit.collider.transform.IsChildOf(transform))
                {
                    float compression=1-hit.distance/suspensionHeight;
                    float verticalSpeed=Vector3.Dot(rb.GetPointVelocity(point.position),transform.up);
                    rb.AddForceAtPosition(transform.up*Mathf.Max(0,compression*suspensionSpring-verticalSpeed*suspensionDamping),
                        point.position,ForceMode.Force);
                }
            }
            if(Vector3.Dot(transform.up,Vector3.up)<-.25f && rb.linearVelocity.magnitude<2f)
                upsideDownTime+=dt;
            else upsideDownTime=0f;
            if(upsideDownTime>=rolloverRecoverySeconds)RecoverVehicle();
        }
        public void RecoverVehicle()
        {
            upsideDownTime=0;
            rb.linearVelocity=Vector3.zero;
            rb.angularVelocity=Vector3.zero;
            transform.position+=Vector3.up*1.5f;
            transform.rotation=Quaternion.Euler(0,transform.eulerAngles.y,0);
        }
        public void SetDriver(IcoThirdPersonPlayer player)
        {
            driver=player;
            if(driver)
            {
                rb.isKinematic=false;
                rb.constraints=RigidbodyConstraints.None;
                rb.WakeUp();
                Debug.Log("Ico RP: motorista entrou. W frente, S re, Espaco freio, H buzina, L farol, F reparar, R desvirar.");
            }
            else{throttle=steer=0;brake=false;if(horn)horn.Stop();}
        }
    }

    public class IcoVehicleInteraction : MonoBehaviour
    {
        public float enterDistance=4f;
        IcoThirdPersonPlayer player;
        IcoVehicle driving;
        CharacterController controller;
        IcoFollowCamera follow;
        readonly System.Collections.Generic.Dictionary<Renderer,bool> rendererStates = new System.Collections.Generic.Dictionary<Renderer,bool>();
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
            rendererStates.Clear();
            foreach(var r in GetComponentsInChildren<Renderer>(true))
            {
                rendererStates[r]=r.enabled;
                r.enabled=false;
            }
            transform.SetParent(car.transform);
            transform.position=car.seat?car.seat.position:car.transform.TransformPoint(Vector3.up*.9f);
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
            foreach(var pair in rendererStates)
                if(pair.Key)pair.Key.enabled=pair.Value;
            rendererStates.Clear();
            controller.enabled=true;
            player.enabled=true;
            car.SetDriver(null);
            driving=null;
            if(follow)follow.target=transform;
        }
    }
}
