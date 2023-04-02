using Unity.Netcode.Components;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Unity.Multiplayer.Samples.Utilities.ClientAuthority {

    [DisallowMultipleComponent]
    public class ClientNetworkTransform : NetworkTransform {
        protected override bool OnIsServerAuthoritative() {
            return false;
        }
    }
}