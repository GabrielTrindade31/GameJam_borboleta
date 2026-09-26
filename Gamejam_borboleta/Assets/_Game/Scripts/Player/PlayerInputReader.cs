using UnityEngine;
using UnityEngine.InputSystem;

namespace ButterflyStep
{
    public class PlayerInputReader : MonoBehaviour
    {
        [Tooltip("Asset de controles. Abra-o para trocar as teclas (Q/E/F/Espaço...).")]
        [SerializeField] private InputActionAsset controls;
        [SerializeField] private string mapName = "Gameplay";

        private InputAction move;
        private InputAction jump;
        private InputAction interact;
        private InputAction attack;
        private InputAction timeBack;
        private InputAction timeForward;
        private InputAction restart;
        private InputAction peek;
        private InputAction stasis;
        private InputAction shoot;
        private InputActionMap map;

        public float MoveX => move != null ? move.ReadValue<Vector2>().x : 0f;
        public float MoveY => move != null ? move.ReadValue<Vector2>().y : 0f;
        public bool JumpPressed => Pressed(jump);
        public bool JumpHeld => jump != null && jump.IsPressed();
        public bool InteractPressed => Pressed(interact);
        public bool AttackPressed => Pressed(attack);
        public bool TimeBackPressed => Pressed(timeBack);
        public bool TimeForwardPressed => Pressed(timeForward);
        public bool RestartPressed => Pressed(restart);
        public bool PeekHeld => peek != null && peek.IsPressed();
        public bool StasisPressed => Pressed(stasis);
        public bool ShootPressed => Pressed(shoot);

        public string BindingName(string action)
        {
            var a = controls != null ? controls.FindAction($"{mapName}/{action}") : null;
            return a != null ? a.GetBindingDisplayString(0) : action;
        }

        public void SetControls(InputActionAsset asset) => controls = asset;

        private void Awake()
        {
            if (controls == null) return;
            map = controls.FindActionMap(mapName, true);
            move = map.FindAction("Move");
            jump = map.FindAction("Jump");
            interact = map.FindAction("Interact");
            attack = map.FindAction("Attack");
            timeBack = map.FindAction("TimeBack");
            timeForward = map.FindAction("TimeForward");
            restart = map.FindAction("Restart");
            peek = map.FindAction("Peek");
            stasis = map.FindAction("Stasis");
            shoot = map.FindAction("Shoot");
        }

        private void OnEnable()
        {
            if (controls != null) controls.FindActionMap(mapName)?.Enable();
        }

        private void OnDisable()
        {
            if (controls != null) controls.FindActionMap(mapName)?.Disable();
        }

        private void Update()
        {
            if (map != null && !map.enabled) map.Enable();
        }

        private static bool Pressed(InputAction action) => action != null && action.WasPressedThisFrame();
    }
}
