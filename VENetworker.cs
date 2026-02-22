using System.Text;
using System.Security.Cryptography;
using Unity.Netcode;
using UnityEngine;
using GameNetcodeStuff;

namespace volatileEmployees
{
    // class for handling networking - sending explosions to all clients
    // source for init/create: https://github.com/ButteryStancakes/ButteRyBalance/blob/master/Network/BRBNetworker.cs
    internal class VENetworker : NetworkBehaviour
    {
        internal static VENetworker Instance { get; private set; }
        internal static GameObject prefab;

        internal static void Init()
        {
            // if the prefab already exists, skip this method; otherwise continue
            if (prefab != null)
            {
                Plugin.mls.LogDebug("Network handler already initialized!");
                return;
            }
            try
            {
                // create prefab object to hold the network objects and set hideFlags to ensure it is not unloaded if unused
                prefab = new(nameof(VENetworker))
                {
                    hideFlags = HideFlags.HideAndDontSave
                };

                // add networkobject to prefab,
                NetworkObject netObj = prefab.AddComponent<NetworkObject>();
                // MD5 creates hash algorithm -> computes hash from UTF8 byte array of (string of (name of assembly + prefab name)) -> unique hash,
                byte[] hash = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(typeof(VENetworker).Assembly.GetName().Name + prefab.name));
                // convert hash to 32bit integer (starting at index 0) and set networkobject's id to that
                netObj.GlobalObjectIdHash = System.BitConverter.ToUInt32(hash, 0);

                // add network behaviour to prefab object
                prefab.AddComponent<VENetworker>();
                // access networkmanager's instance and add prefab object
                NetworkManager.Singleton.AddNetworkPrefab(prefab);

                Plugin.mls.LogDebug("Successfully created VENetworker!");
            }
            catch(System.Exception e)
            {
                Plugin.mls.LogError($"Something went wrong!\n{e}\n{Plugin.modName} will not work!");
            }

        }

        internal static void Create()
        {
            try
            {
                // if the network is the server and the prefab was created, then copy the prefab, get the networkobject out of it, and spawn it (destroyed with scene)
                if (NetworkManager.Singleton.IsServer && prefab != null)
                {
                    Instantiate(prefab).GetComponent<NetworkObject>().Spawn(true);
                }
            }
            catch (System.Exception e)
            {
                Plugin.mls.LogError($"Something went wrong!\n{e}\n{Plugin.modName} will not work!");
            }
        }

        void Awake()
        {
            Instance = this;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            // if incorrect instance was assigned, reassign to this
            if (Instance != this)
            {
                // if two instances exist, destroy the current one
                if (Instance.TryGetComponent(out NetworkObject netObj) && !netObj.IsSpawned && Instance != prefab)
                {
                    Destroy(Instance);
                }

                Plugin.mls.LogWarning($"2 {nameof(VENetworker)} objects exist! Reassigning to correct instance.");
                Instance = this;
            }
            Plugin.mls.LogDebug("Successfully spawned network object!");
        }


        [Rpc(SendTo.Everyone)]
        public void SpawnExplosionPlayerRpc(NetworkObjectReference player)
        {
            if (player.TryGet(out NetworkObject netObj)) 
            {
                Plugin.mls.LogDebug($"Player: {netObj}");
                Landmine.SpawnExplosion(netObj.GetComponent<PlayerControllerB>().transform.position, spawnExplosionEffect: true, killRange: 5.7f, damageRange: 6f, physicsForce: 2f);
                return;
            }
        }

        public static void SpawnExplosionEnemy(NetworkObject enemy)
        {
            VENetworker.Instance.SpawnExplosionEnemyRpc(enemy);
        }

        [Rpc(SendTo.Everyone)]
        public void SpawnExplosionEnemyRpc(NetworkObjectReference enemy)
        {
            if (enemy.TryGet(out NetworkObject netObj))
            {
                Plugin.mls.LogDebug($"Enemy: {netObj}");
                Landmine.SpawnExplosion(netObj.GetComponent<EnemyAI>().transform.position, spawnExplosionEffect: true, killRange: 5.7f, damageRange: 6f, physicsForce: 2f);
                return;
            }
            Plugin.mls.LogDebug($"No netObj found!");
        }

        public static void SpawnExplosion5x(NetworkObject enemy)
        {
            VENetworker.Instance.SpawnExplosion5xRpc(enemy);
        }

        [Rpc(SendTo.Everyone)]
        public void SpawnExplosion5xRpc(NetworkObjectReference enemy)
        {
            if (enemy.TryGet(out NetworkObject netObj))
            {
                Plugin.mls.LogDebug($"Enemy: {netObj}");
                Vector3 pos = netObj.GetComponent<EnemyAI>().transform.position;
                Landmine.SpawnExplosion(pos, spawnExplosionEffect: true, killRange: 5.7f, damageRange: 6f, physicsForce: 2f);
                Landmine.SpawnExplosion(pos, spawnExplosionEffect: true, killRange: 5.7f, damageRange: 6f, physicsForce: 2f);
                Landmine.SpawnExplosion(pos, spawnExplosionEffect: true, killRange: 5.7f, damageRange: 6f, physicsForce: 2f);
                Landmine.SpawnExplosion(pos, spawnExplosionEffect: true, killRange: 5.7f, damageRange: 6f, physicsForce: 2f);
                Landmine.SpawnExplosion(pos, spawnExplosionEffect: true, killRange: 5.7f, damageRange: 6f, physicsForce: 2f);
                return;
            }
            Plugin.mls.LogDebug($"No netObj found!");
        }

        public static void DespawnEnemy(NetworkObject enemy)
        {
            VENetworker.Instance.DespawnEnemyRpc(enemy);
        }

        [Rpc(SendTo.Owner)]
        public void DespawnEnemyRpc(NetworkObjectReference enemy)
        {
            if (enemy.TryGet(out NetworkObject netObj))
            {
                Plugin.mls.LogDebug($"Enemy: {netObj}");
                netObj.Despawn();
                return;
            }
            Plugin.mls.LogDebug($"No netObj found!");
        }
    }
}
