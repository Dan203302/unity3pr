using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputSystem_Actions
{
    private InputActionAsset asset;
    public PlayerActions Player { get; private set; }

    public InputSystem_Actions()
    {
        string json = System.IO.File.ReadAllText(Application.dataPath + "/InputSystem_Actions.inputactions");
        asset = InputActionAsset.FromJson(json);
        Player = new PlayerActions(asset.FindActionMap("Player"));
    }

    public void Enable()
    {
        asset?.Enable();
    }

    public void Disable()
    {
        asset?.Disable();
    }

    public class PlayerActions
    {
        private InputActionMap map;

        public InputAction Move => map.FindAction("Move");
        public InputAction Look => map.FindAction("Look");
        public InputAction Attack => map.FindAction("Attack");
        public InputAction Interact => map.FindAction("Interact");
        public InputAction Crouch => map.FindAction("Crouch");
        public InputAction Jump => map.FindAction("Jump");
        public InputAction Previous => map.FindAction("Previous");
        public InputAction Next => map.FindAction("Next");
        public InputAction Sprint => map.FindAction("Sprint");
        public InputAction SpawnObject => map.FindAction("SpawnObject");
        public InputAction RaycastInteract => map.FindAction("RaycastInteract");

        public PlayerActions(InputActionMap actionMap)
        {
            map = actionMap;
        }

        public void Enable()
        {
            map?.Enable();
        }

        public void Disable()
        {
            map?.Disable();
        }
    }
}
