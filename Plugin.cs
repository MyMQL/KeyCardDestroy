using System;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Doors;
using Exiled.Events.EventArgs;
using Exiled.Events.EventArgs.Player;
using MEC;
using UnityEngine;

namespace SCP_SL_DestroyCardReader
{
    public class Plugin : Plugin<Config>
    {
        public override string Name => "DestroyCardReader";
        public override string Author => "MyMQL";
        public override Version Version => new Version(1, 0, 0);
        public override Version RequiredExiledVersion => new Version(8, 14, 0);

        public static Plugin Instance { get; private set; }

        public override void OnEnabled()
        {
            Instance = this;

            Exiled.Events.Handlers.Player.Shooting += OnShooting;

            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            Exiled.Events.Handlers.Player.Shooting -= OnShooting;

            Instance = null;

            base.OnDisabled();
        }

        private void OnShooting(ShootingEventArgs ev)
        {
            Log.Info($"Gracz {ev.Player.Nickname} strzelił.");

            // Parametry sfery
            Vector3 origin = ev.Player.CameraTransform.position;
            Vector3 direction = ev.Player.CameraTransform.forward;
            float sphereRadius = 0.75f; // Promień sfery
            float maxDistance = 3f; // Maksymalny zasięg

            // Wykonanie SphereCast
            if (Physics.SphereCast(origin, sphereRadius, direction, out RaycastHit hit, maxDistance))
            {
                Log.Info($"Trafiono obiekt: {hit.collider.gameObject.name}");

                // Sprawdzenie, czy trafiono czytnik kart
                if (hit.collider != null && hit.collider.gameObject.name.Contains("TouchScreenPanel"))
                {
                    Log.Info("Trafiono czytnik kart!");

                    // Szukaj drzwi w pobliżu trafionego obiektu
                    Door door = FindDoorNearby(hit.collider.gameObject.transform.position);
                    if (door == null)
                    {
                        Log.Info("Nie znaleziono drzwi powiązanych z czytnikiem.");
                        return;
                    }

                    // Zablokuj drzwi
                    door.ChangeLock(DoorLockType.AdminCommand);
                    Log.Info($"Zablokowano drzwi: {door.Name} na {Config.DestroyDuration} sekund.");

                    // Odblokuj drzwi po upływie czasu
                    Timing.CallDelayed(Config.DestroyDuration, () =>
                    {
                        if (door != null)
                        {
                            door.ChangeLock(DoorLockType.None);
                            Log.Info($"Drzwi {door.Name} zostały odblokowane.");
                        }
                    });

                    // info dla gracza, na 3 sekundy domyslnie
                    ev.Player.ShowHint("You have broken the card reader!", 3f);
                }
                else
                {
                    Log.Info("Trafiony obiekt nie jest czytnikiem kart.");
                }
            }
            else
            {
                Log.Info("Nie trafiono żadnego obiektu.");
            }
        }

        // jezeli drzwi == null, to znajdz najblisze
        private Door FindDoorNearby(Vector3 position)
        {
            return Door.List
                       .OrderBy(door => Vector3.Distance(door.Position, position))
                       .FirstOrDefault(door => Vector3.Distance(door.Position, position) < 2f);
        }
    }
}



