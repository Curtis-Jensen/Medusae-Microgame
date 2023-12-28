using System.Collections.Generic;
using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.Events;

namespace Unity.FPS.Gameplay
{
    [RequireComponent(typeof(PlayerInputHandler))]
    public class PlayerWeaponsManager : MonoBehaviour
    {
        #region🌎 Variables
        public enum WeaponSwitchState
        {
            Up,
            Down,
            PutDownPrevious,
            PutUpNew,
        }

        public bool randomizeWeapons;

        [Tooltip("List of weapon the player could start with")]
        public List<WeaponController> possibleWeapons = new List<WeaponController>();
        [Tooltip("What the player currently has equipped")]
        public WeaponController[] weaponSlots;

        [Header("References")]
        [Tooltip("Secondary camera used to avoid seeing weapon go throw geometries")]
        public Camera WeaponCamera;

        [Tooltip("Parent transform where all weapon will be added in the hierarchy")]
        public Transform WeaponParentSocket;

        [Tooltip("Position for weapons when active but not actively aiming")]
        public Transform DefaultWeaponPosition;

        [Tooltip("Position for melee weapons when active but not swinging")]
        public Transform defaultMeleePosition;

        [Tooltip("Position for weapons when aiming")]
        public Transform AimingWeaponPosition;

        [Tooltip("Position for innactive weapons")]
        public Transform DownWeaponPosition;

        [Tooltip("Position for reversed weapons")]
        public Transform reverseWeaponPosition;

        [Header("Weapon Bob")]
        [Tooltip("Frequency at which the weapon will move around in the screen when the player is in movement")]
        public float BobFrequency = 10f;

        [Tooltip("How fast the weapon bob is applied, the bigger value the fastest")]
        public float BobSharpness = 10f;

        [Tooltip("Distance the weapon bobs when not aiming")]
        public float DefaultBobAmount = 0.05f;

        [Tooltip("Distance the weapon bobs when aiming")]
        public float AimingBobAmount = 0.02f;

        [Header("Weapon Recoil")]
        [Tooltip("This will affect how fast the recoil moves the weapon, the bigger the value, the faster")]
        public float RecoilSharpness = 50f;

        [Tooltip("Maximum distance the recoil can affect the weapon")]
        public float MaxRecoilDistance = 0.5f;

        [Tooltip("How fast the weapon goes back to it's original position after the recoil is finished")]
        public float RecoilRestitutionSharpness = 10f;

        [Header("Misc")]
        [Tooltip("Speed at which the aiming animatoin is played")]
        public float AimingAnimationSpeed = 10f;

        [Tooltip("Field of view when not aiming")]
        public float DefaultFov = 60f;

        [Tooltip("Portion of the regular FOV to apply to the weapon camera")]
        public float WeaponFovMultiplier = 1f;

        [Tooltip("Delay before switching weapon a second time, to avoid recieving multiple inputs from mouse wheel")]
        public float WeaponSwitchDelay = 1f;

        [Tooltip("Layer to set FPS weapon gameObjects to")]
        public LayerMask FpsWeaponLayer;

        public bool IsAiming { get; private set; }
        public bool IsPointingAtEnemy { get; private set; }
        public int ActiveWeaponIndex { get; private set; }

        public UnityAction<WeaponController> OnSwitchedToWeapon;
        public UnityAction<WeaponController, int> OnAddedWeapon;
        public UnityAction<WeaponController, int> OnRemovedWeapon;

        PlayerInputHandler inputHandler;
        PlayerCharacterController playerCharacterController;
        float weaponBobFactor;
        Vector3 lastCharacterPosition;
        Vector3 weaponMainLocalPosition;
        Vector3 weaponBobLocalPosition;
        Vector3 weaponRecoilLocalPosition;
        Vector3 accumulatedRecoil;
        Quaternion weaponMainLocalRotation;
        float timeStartedWeaponSwitch;
        WeaponSwitchState weaponSwitchState;
        int weaponSwitchNewWeaponIndex;
        #endregion

        #region Monobehavior Methods
        void Start()
        {
            ActiveWeaponIndex = -1;
            weaponSwitchState = WeaponSwitchState.Down;

            inputHandler = GetComponent<PlayerInputHandler>();
            DebugUtility.HandleErrorIfNullGetComponent<PlayerInputHandler, PlayerWeaponsManager>(inputHandler, this,
                gameObject);

            playerCharacterController = GetComponent<PlayerCharacterController>();
            DebugUtility.HandleErrorIfNullGetComponent<PlayerCharacterController, PlayerWeaponsManager>(
                playerCharacterController, this, gameObject);

            SetFov(DefaultFov);

            OnSwitchedToWeapon += OnWeaponSwitched;

            PickFirstWeapons();

            SwitchWeapon(true);
        }

        void PickFirstWeapons()
        {
            for (int i = 0; i < weaponSlots.Length; i++)
            {
                if (randomizeWeapons)
                {
                    int randomIndex = Random.Range(0, possibleWeapons.Count);
                    var selectedWeapon = possibleWeapons[randomIndex];

                    AddWeapon(selectedWeapon);
                    possibleWeapons.RemoveAt(randomIndex);
                }
                else if (i < possibleWeapons.Count)
                {
                    AddWeapon(possibleWeapons[i]);
                }
                else
                {
                    Debug.LogError($"There are more weapons expected ({weaponSlots.Length}) than what are loaded via weapons ({possibleWeapons.Count})");            
                    break; // Break the loop if there are no more available weapons to add
                }
            }
        }

        /* 10 shoot handling
         * 
         * 15 If the weapon is null (As in the player has no weapon) or the weapon is currently reloading, don't try to actively do anything with the weapon.
         * 
         * 20 handle aiming down sights
         * 
         * 30 handle shooting
         * 
         * 40 Handle accumulating recoil if the gun has fired
         * 
         * 45 If the accumulated recoil is too much it will be clamped
         * 
         * 50 Weapon switch handling
         * 
         * 60 Pointing at enemy handling for the crosshair
         */
        void Update()
        {
            WeaponController activeWeapon = GetActiveWeapon(); // 10

            // 15
            if (activeWeapon == null)
                return;

            if (weaponSwitchState == WeaponSwitchState.Up)
            {
                IsAiming = inputHandler.GetAimInputHeld(); // 20

                bool hasAttacked = activeWeapon.HandleAttackInputs( // 30
                    inputHandler.GetFireInputDown(),
                    inputHandler.GetFireInputHeld(),
                    inputHandler.GetFireInputReleased());

                if (hasAttacked) // 40
                {
                    accumulatedRecoil += Vector3.back * activeWeapon.AttackAnimation();
                    accumulatedRecoil = Vector3.ClampMagnitude(accumulatedRecoil, MaxRecoilDistance);// 45
                }
            }

            // 50
            bool weaponResting = weaponSwitchState == WeaponSwitchState.Up || weaponSwitchState == WeaponSwitchState.Down;

            bool weaponPreOccupied = IsAiming || activeWeapon.IsCharging || !weaponResting;

            if (!weaponPreOccupied)
            {
                int switchWeaponInput = inputHandler.GetSwitchWeaponInput();
                if (switchWeaponInput != 0)
                {
                    bool switchUp = switchWeaponInput > 0;
                    SwitchWeapon(switchUp);
                }
                else
                {
                    switchWeaponInput = inputHandler.GetSelectWeaponInput();
                    if (switchWeaponInput != 0)
                        if (GetWeaponAtSlotIndex(switchWeaponInput - 1) != null)
                            SwitchToWeaponIndex(switchWeaponInput - 1);
                }
            }

            // 60
            IsPointingAtEnemy = false;

            if (Physics.Raycast(WeaponCamera.transform.position, WeaponCamera.transform.forward, out RaycastHit hit,
                1000, -1, QueryTriggerInteraction.Ignore))
                if (hit.collider.GetComponentInParent<Health>() != null)
                    IsPointingAtEnemy = true;
        }

        /// <summary>
        /// Update various animated features in LateUpdate because it needs to override the animated arm position
        /// </summary>
        void LateUpdate()
        {
            UpdateWeaponAiming();
            UpdateWeaponBob();
            UpdateWeaponRecoil();
            UpdateWeaponSwitching();

            //Debug.Log(weaponRecoilLocalPosition);

            // Set final weapon socket position based on all the combined animation influences
            WeaponParentSocket.localPosition =
                weaponMainLocalPosition + weaponBobLocalPosition + weaponRecoilLocalPosition;

            WeaponParentSocket.localRotation = weaponMainLocalRotation;
        }
        #endregion

        /// <summary>
        /// Sets the FOV of the main camera and the weapon camera simultaneously
        /// </summary>
        /// <param name="fov"></param>
        public void SetFov(float fov)
        {
            playerCharacterController.PlayerCamera.fieldOfView = fov;
            WeaponCamera.fieldOfView = fov * WeaponFovMultiplier;
        }

        /// <summary>
        /// Iterate on all weapon slots to find the next valid weapon to switch to
        /// </summary>
        /// <param name="ascendingOrder"></param>
        public void SwitchWeapon(bool ascendingOrder)
        {
            int newWeaponIndex = -1;
            int closestSlotDistance = weaponSlots.Length;
            for (int i = 0; i < weaponSlots.Length; i++)
            {
                // If the weapon at this slot is valid, calculate its "distance" from the active slot index (either in ascending or descending order)
                // and select it if it's the closest distance yet
                if (i != ActiveWeaponIndex && GetWeaponAtSlotIndex(i) != null)
                {
                    int distanceToActiveIndex = GetDistanceBetweenWeaponSlots(ActiveWeaponIndex, i, ascendingOrder);

                    if (distanceToActiveIndex < closestSlotDistance)
                    {
                        closestSlotDistance = distanceToActiveIndex;
                        newWeaponIndex = i;
                    }
                }
            }

            // Handle switching to the new weapon index
            SwitchToWeaponIndex(newWeaponIndex);
        }

        /// <summary>
        /// Switches to the given weapon index in weapon slots if the new index is a valid weapon that is different from our current one
        /// </summary>
        /// <param name="newWeaponIndex"></param>
        /// <param name="force"></param>
        public void SwitchToWeaponIndex(int newWeaponIndex, bool force = false)
        {
            if (force || (newWeaponIndex != ActiveWeaponIndex && newWeaponIndex >= 0))
            {
                // Store data related to weapon switching animation
                weaponSwitchNewWeaponIndex = newWeaponIndex;
                timeStartedWeaponSwitch = Time.time;

                // Handle case of switching to a valid weapon for the first time (simply put it up without putting anything down first)
                if (GetActiveWeapon() == null)
                {
                    weaponMainLocalPosition = DownWeaponPosition.localPosition;
                    weaponSwitchState = WeaponSwitchState.PutUpNew;
                    ActiveWeaponIndex = weaponSwitchNewWeaponIndex;

                    WeaponController newWeapon = GetWeaponAtSlotIndex(weaponSwitchNewWeaponIndex);
                    if (OnSwitchedToWeapon != null)
                        OnSwitchedToWeapon.Invoke(newWeapon);
                }
                // otherwise, remember we are putting down our current weapon for switching to the next one
                else
                    weaponSwitchState = WeaponSwitchState.PutDownPrevious;
            }
        }

        public WeaponController HasWeapon(WeaponController weaponPrefab)
        {
            // Checks if we already have a weapon coming from the specified prefab
            for (var index = 0; index < weaponSlots.Length; index++)
            {
                var w = weaponSlots[index];
                if (w != null && w.sourcePrefab == weaponPrefab.gameObject)
                    return w;
            }

            return null;
        }

        /// <summary>
        /// Updates weapon position and camera FoV for the aiming transition
        /// </summary>
        void UpdateWeaponAiming()
        {
            if (weaponSwitchState != WeaponSwitchState.Up) return;

            Transform weaponLocation;
            float fov;

            WeaponController activeWeapon = GetActiveWeapon();

            fov = DefaultFov; // Settting the default FOV; may be changed if aiming, but otherwise it will stay the same

            // If the player is aiming down sights
            if (IsAiming && activeWeapon)
            {
                weaponLocation = AimingWeaponPosition;
                fov = activeWeapon.AimZoomRatio * DefaultFov;
            }
            else if (Input.GetButton(InputNames.buttonReverseAim)) // If the player is aiming behind themselves
                weaponLocation = reverseWeaponPosition;
            else if (activeWeapon.meleeWeapon) // If it's a melee weapon
                weaponLocation = defaultMeleePosition;
            else  // If the player is hip firing
                weaponLocation = DefaultWeaponPosition;

            weaponMainLocalPosition = Vector3.Lerp(weaponMainLocalPosition,
                weaponLocation.localPosition + activeWeapon.AimOffset,
                AimingAnimationSpeed * Time.deltaTime);

            weaponMainLocalRotation = Quaternion.Lerp(weaponMainLocalRotation,
                weaponLocation.localRotation, AimingAnimationSpeed * Time.deltaTime);

            SetFov(Mathf.Lerp(playerCharacterController.PlayerCamera.fieldOfView,
                fov, AimingAnimationSpeed * Time.deltaTime));
        }

        /// <summary>
        /// Updates the weapon bob animation based on character speed
        /// </summary>
        void UpdateWeaponBob()
        {
            if (Time.deltaTime > 0f)
            {
                Vector3 playerCharacterVelocity =
                    (playerCharacterController.transform.position - lastCharacterPosition) / Time.deltaTime;

                // calculate a smoothed weapon bob amount based on how close to our max grounded movement velocity we are
                float characterMovementFactor = 0f;
                if (playerCharacterController.IsGrounded)
                {
                    characterMovementFactor =
                        Mathf.Clamp01(playerCharacterVelocity.magnitude /
                                      (playerCharacterController.MaxSpeedOnGround *
                                       playerCharacterController.SprintSpeedModifier));
                }

                weaponBobFactor =
                    Mathf.Lerp(weaponBobFactor, characterMovementFactor, BobSharpness * Time.deltaTime);

                // Calculate vertical and horizontal weapon bob values based on a sine function
                float bobAmount = IsAiming ? AimingBobAmount : DefaultBobAmount;
                float frequency = BobFrequency;
                float hBobValue = Mathf.Sin(Time.time * frequency) * bobAmount * weaponBobFactor;
                float vBobValue = ((Mathf.Sin(Time.time * frequency * 2f) * 0.5f) + 0.5f) * bobAmount *
                                  weaponBobFactor;

                // Apply weapon bob
                weaponBobLocalPosition.x = hBobValue;
                weaponBobLocalPosition.y = Mathf.Abs(vBobValue);

                lastCharacterPosition = playerCharacterController.transform.position;
            }
        }

        /// <summary>
        /// Updates the weapon recoil animation
        /// </summary>
        void UpdateWeaponRecoil()
        {
            //Debug.Log($"weaponRecoilLocalPosition.z = {weaponRecoilLocalPosition.z}, & accumulatedRecoil.z = {accumulatedRecoil.z}");

            // if the accumulated recoil is further away from the current position, make the current position move towards the recoil target
            if (weaponRecoilLocalPosition.z >= accumulatedRecoil.z * 0.99f)
            {
                weaponRecoilLocalPosition = Vector3.Lerp(weaponRecoilLocalPosition, accumulatedRecoil,
                    RecoilSharpness * Time.deltaTime);
            }
            // otherwise, move recoil position to make it recover towards its resting pose
            else
            {
                weaponRecoilLocalPosition = Vector3.Lerp(weaponRecoilLocalPosition, Vector3.zero,
                    RecoilRestitutionSharpness * Time.deltaTime);
                accumulatedRecoil = weaponRecoilLocalPosition;
            }
        }

        /// <summary>
        /// Updates the animated transition of switching weapons
        /// </summary>
        void UpdateWeaponSwitching()
        {
            // Calculate the time ratio (0 to 1) since weapon switch was triggered
            float switchingTimeFactor = 0f;
            if (WeaponSwitchDelay == 0f)
            {
                switchingTimeFactor = 1f;
            }
            else
            {
                switchingTimeFactor = Mathf.Clamp01((Time.time - timeStartedWeaponSwitch) / WeaponSwitchDelay);
            }

            // Handle transiting to new switch state
            if (switchingTimeFactor >= 1f)
            {
                if (weaponSwitchState == WeaponSwitchState.PutDownPrevious)
                {
                    // Deactivate old weapon
                    var oldWeapon = GetWeaponAtSlotIndex(ActiveWeaponIndex);
                    if (oldWeapon != null)
                        oldWeapon.ShowWeapon(false);

                    ActiveWeaponIndex = weaponSwitchNewWeaponIndex;
                    switchingTimeFactor = 0f;

                    // Activate new weapon
                    var newWeapon = GetWeaponAtSlotIndex(ActiveWeaponIndex);
                    if (OnSwitchedToWeapon != null)
                        OnSwitchedToWeapon.Invoke(newWeapon);

                    if (newWeapon)
                    {
                        timeStartedWeaponSwitch = Time.time;
                        weaponSwitchState = WeaponSwitchState.PutUpNew;
                    }
                    else
                        // if new weapon is null, don't follow through with putting weapon back up
                        weaponSwitchState = WeaponSwitchState.Down;
                }
                else if (weaponSwitchState == WeaponSwitchState.PutUpNew)
                    weaponSwitchState = WeaponSwitchState.Up;
            }

            // Handle moving the weapon socket position for the animated weapon switching
            if (weaponSwitchState == WeaponSwitchState.PutDownPrevious)
            {
                weaponMainLocalPosition = Vector3.Lerp(DefaultWeaponPosition.localPosition,
                    DownWeaponPosition.localPosition, switchingTimeFactor);
            }
            else if (weaponSwitchState == WeaponSwitchState.PutUpNew)
            {
                weaponMainLocalPosition = Vector3.Lerp(DownWeaponPosition.localPosition,
                    DefaultWeaponPosition.localPosition, switchingTimeFactor);
            }
        }

        /// <summary>
        /// Adds a weapon to our inventory
        /// </summary>
        /// <param name="weaponPrefab"></param>
        /// <returns></returns>
        public bool AddWeapon(WeaponController weaponPrefab)
        {
            // 1 if we already hold this weapon type (a weapon coming from the same source prefab), don't add the weapon
            if (HasWeapon(weaponPrefab) != null)
                return false;

            // 2 search our weapon slots for the first free one, assign the weapon to it, and return true if we found one. Return false otherwise
            for (int i = 0; i < weaponSlots.Length; i++)
            {
                // 3 If the weapon slot is not free, move along
                if (weaponSlots[i] != null)
                    continue;

                // 4 Spawn the weapon prefab as child of the weapon socket
                WeaponController weaponInstance = Instantiate(weaponPrefab, WeaponParentSocket);
                weaponInstance.transform.localPosition = Vector3.zero;
                weaponInstance.transform.localRotation = Quaternion.identity;

                // 5 Set owner to this gameObject so the weapon can alter projectile / damage logic accordingly
                weaponInstance.owner = gameObject;
                weaponInstance.sourcePrefab = weaponPrefab.gameObject;
                weaponInstance.ShowWeapon(false);

                // 6 Assign the first person layer to the weapon
                int layerIndex =
                    Mathf.RoundToInt(Mathf.Log(FpsWeaponLayer.value,
                        2)); // 7 This function converts a layermask to a layer index
                foreach (Transform t in weaponInstance.gameObject.GetComponentsInChildren<Transform>(true))
                    t.gameObject.layer = layerIndex;

                weaponSlots[i] = weaponInstance;

                if (OnAddedWeapon != null)
                    OnAddedWeapon.Invoke(weaponInstance, i);

                return true;
            }

            // Handle auto-switching to weapon if no weapons currently
            if (GetActiveWeapon() != null)
            {
                RemoveWeapon(weaponSlots[ActiveWeaponIndex]);

                // 4 Spawn the weapon prefab as child of the weapon socket
                WeaponController weaponInstance = Instantiate(weaponPrefab, WeaponParentSocket);
                weaponInstance.transform.localPosition = Vector3.zero;
                weaponInstance.transform.localRotation = Quaternion.identity;

                // 5 Set owner to this gameObject so the weapon can alter projectile / damage logic accordingly
                weaponInstance.owner = gameObject;
                weaponInstance.sourcePrefab = weaponPrefab.gameObject;
                weaponInstance.ShowWeapon(false);

                // 6 Assign the first person layer to the weapon
                int layerIndex =
                    Mathf.RoundToInt(Mathf.Log(FpsWeaponLayer.value,
                        2)); // 7 This function converts a layermask to a layer index
                foreach (Transform t in weaponInstance.gameObject.GetComponentsInChildren<Transform>(true))
                    t.gameObject.layer = layerIndex;

                weaponSlots[ActiveWeaponIndex] = weaponInstance;

                if (OnAddedWeapon != null)
                    OnAddedWeapon.Invoke(weaponInstance, ActiveWeaponIndex);

                SwitchWeapon(true);

                return true;
            }

            SwitchWeapon(true);

            return false;
        }

        public bool RemoveWeapon(WeaponController weaponInstance)
        {
            // Look through our slots for that weapon
            for (int i = 0; i < weaponSlots.Length; i++)
            {
                // when weapon found, remove it
                if (weaponSlots[i] == weaponInstance)
                {
                    weaponSlots[i] = null;

                    if (OnRemovedWeapon != null)
                        OnRemovedWeapon.Invoke(weaponInstance, i);

                    Destroy(weaponInstance.gameObject);

                    return true;
                }
            }

            return false;
        }

        public WeaponController GetActiveWeapon()
        {
            return GetWeaponAtSlotIndex(ActiveWeaponIndex);
        }

        public WeaponController GetWeaponAtSlotIndex(int index)
        {
            // find the active weapon in our weapon slots based on our active weapon index
            if (index >= 0 &&
                index < weaponSlots.Length)
            {
                return weaponSlots[index];
            }

            // if we didn't find a valid active weapon in our weapon slots, return null
            return null;
        }

        /// <summary>
        /// Calculates the "distance" between two weapon slot indexes
        /// For example: if we had 5 weapon slots, the distance between slots #2 and #4 would be 2 in ascending order, and 3 in descending order
        /// </summary>
        /// <param name="fromSlotIndex"></param>
        /// <param name="toSlotIndex"></param>
        /// <param name="ascendingOrder"></param>
        /// <returns></returns>
        int GetDistanceBetweenWeaponSlots(int fromSlotIndex, int toSlotIndex, bool ascendingOrder)
        {
            int distanceBetweenSlots = 0;

            if (ascendingOrder)
            {
                distanceBetweenSlots = toSlotIndex - fromSlotIndex;
            }
            else
            {
                distanceBetweenSlots = -1 * (toSlotIndex - fromSlotIndex);
            }

            if (distanceBetweenSlots < 0)
            {
                distanceBetweenSlots = weaponSlots.Length + distanceBetweenSlots;
            }

            return distanceBetweenSlots;
        }

        void OnWeaponSwitched(WeaponController newWeapon)
        {
            if (newWeapon == null) return;

            newWeapon.ShowWeapon(true);
        }
    }
}