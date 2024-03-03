using FirstGearGames.FPSLand.Characters.Vitals;
using FishNet.Object;
using UnityEngine;
using UnityEngine.Rendering;

namespace FirstGearGames.FPSLand.Characters.Bodies
{

    /// <summary>
    /// Changes visibility of body gameObject based on ownership.
    /// </summary>
    public class SetBodyPerspective : NetworkBehaviour
    {

        #region Private.
        /// <summary>
        /// BodiesConfigurations on this object.
        /// </summary>
        private BodiesConfigurations _bodiesConfigurations;
        #endregion

        private void Awake()
        {
            _bodiesConfigurations = GetComponent<BodiesConfigurations>();
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            if (!base.IsClient)
                ShowAliveBodies();
        }

        public override void OnStartClient()
        {
            base.OnStartClient();

            NetworkInitialize();
            DisableOwnerShadows();
            ShowAliveBodies();
        }

        /// <summary>
        /// Initializes this script for use. Should only be completed once.
        /// </summary>
        private void NetworkInitialize()
        {
            if (base.IsOwner)
            {
                Health health = GetComponent<Health>();
                health.OnDeath += Health_OnDeath;
                health.OnRespawned += Health_OnRespawned;
            }
        }

        /// <summary>
        /// Received when the character is respawned.
        /// </summary>
        private void Health_OnRespawned()
        {
            ShowAliveBodies();
        }

        /// <summary>
        /// Received when the character is dead.
        /// </summary>
        private void Health_OnDeath()
        {
            ShowDeadBodies();
        }

        /// <summary>
        /// Configures shadows and visibility on renderers based on client and server roles.
        /// </summary>
        private void DisableOwnerShadows()
        {
            //Only configure renderers if owner.
            if (base.IsOwner)
            {
                SkinnedMeshRenderer[] skinnedMeshRenderers;
                MeshRenderer[] meshRenderers;
                /* If client host then third person shadows must be hidden as well. */
                if (base.IsClient && base.IsServer)
                {
                    //Body parts.
                    skinnedMeshRenderers = _bodiesConfigurations.ThirdPersonObject.GetComponentsInChildren<SkinnedMeshRenderer>();
                    for (int i = 0; i < skinnedMeshRenderers.Length; i++)
                        skinnedMeshRenderers[i].shadowCastingMode = ShadowCastingMode.Off;
                    //Weapons.
                    meshRenderers = _bodiesConfigurations.ThirdPersonObject.GetComponentsInChildren<MeshRenderer>();
                    for (int i = 0; i < meshRenderers.Length; i++)
                        meshRenderers[i].shadowCastingMode = ShadowCastingMode.Off;
                }

                /* First person shadows always hide for owner. */
                //Body parts.
                skinnedMeshRenderers = _bodiesConfigurations.FirstPersonObject.GetComponentsInChildren<SkinnedMeshRenderer>();
                for (int i = 0; i < skinnedMeshRenderers.Length; i++)
                    skinnedMeshRenderers[i].shadowCastingMode = ShadowCastingMode.Off;
                //Weapons.
                meshRenderers = _bodiesConfigurations.FirstPersonObject.GetComponentsInChildren<MeshRenderer>();
                for (int i = 0; i < meshRenderers.Length; i++)
                    meshRenderers[i].shadowCastingMode = ShadowCastingMode.Off;
            }
        }

        /// <summary>
        /// Shows bodies and renderers for when there is health remaining.
        /// </summary>
        public void ShowAliveBodies()
        {
            //Enable first person based on if owner.
            _bodiesConfigurations.FirstPersonObject.SetActive(base.IsOwner);

            //Has authority.
            if (base.IsOwner)
            {
                //If client host must also activate third person, but hide renderers.
                if (base.IsClient && base.IsServer)
                {
                    SkinnedMeshRenderer[] skinnedMeshRenderers;
                    MeshRenderer[] meshRenderers;
                    /* If client host then third person shadows must be hidden as well. */
                    if (base.IsClient && base.IsServer)
                    {
                        //Body parts.
                        skinnedMeshRenderers = _bodiesConfigurations.ThirdPersonObject.GetComponentsInChildren<SkinnedMeshRenderer>();
                        for (int i = 0; i < skinnedMeshRenderers.Length; i++)
                            skinnedMeshRenderers[i].enabled = false;
                        //Weapons.
                        meshRenderers = _bodiesConfigurations.ThirdPersonObject.GetComponentsInChildren<MeshRenderer>();
                        for (int i = 0; i < meshRenderers.Length; i++)
                            meshRenderers[i].enabled = false;
                    }
                }
                //Not a client host. Must disable third person.
                else
                {
                    _bodiesConfigurations.ThirdPersonObject.SetActive(false);
                }
            }
            //Does not have authority. Must show third person.
            else
            {
                _bodiesConfigurations.ThirdPersonObject.SetActive(true);
            }
        }

        /// <summary>
        /// Shows bodies and renderers for when health is depleted.
        /// </summary>
        private void ShowDeadBodies()
        {
            //Disable first person based on if owner.
            _bodiesConfigurations.FirstPersonObject.SetActive(!base.IsOwner);

            //Has authority.
            if (base.IsOwner)
            {
                //If client host must also show third person renderers.
                if (base.IsClient && base.IsServer)
                {
                    SkinnedMeshRenderer[] skinnedMeshRenderers;
                    MeshRenderer[] meshRenderers;
                    /* If client host then third person shadows must be hidden as well. */
                    if (base.IsClient && base.IsServer)
                    {
                        //Body parts.
                        skinnedMeshRenderers = _bodiesConfigurations.ThirdPersonObject.GetComponentsInChildren<SkinnedMeshRenderer>();
                        for (int i = 0; i < skinnedMeshRenderers.Length; i++)
                            skinnedMeshRenderers[i].enabled = true;
                        //Weapons.
                        meshRenderers = _bodiesConfigurations.ThirdPersonObject.GetComponentsInChildren<MeshRenderer>();
                        for (int i = 0; i < meshRenderers.Length; i++)
                            meshRenderers[i].enabled = true;
                    }
                }
                //Not a client host. Must enable. third person.
                else
                {
                    _bodiesConfigurations.ThirdPersonObject.SetActive(true);
                }
            }
            /* Does not have authority. No additional action is needed
             * if player doesn't have authority as third person is already
             * shown. */
            else { }
        }

    }


}