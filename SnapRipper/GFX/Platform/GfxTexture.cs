using UnityEngine;

namespace VirtualPhenix.Nintendo64
{
    public class GfxTexture : GfxTextureDescriptor, GfxTextureImpl, GfxResource
    {
        [SerializeField] private string m_resourceName;
        [SerializeField] private long m_id;

        public string ResourceName { get => m_resourceName; set => m_resourceName = value; }
        public long ResourceUniqueId { get => m_id; set => m_id = value; }
    }
}