using System.Collections.Generic;
using UnityEngine;

namespace TheGame
{
    public class ThrowablePicker : MonoBehaviour, IPicker<IThrowable>
    {
        private List<IPickupable<IThrowable>> m_PickablesInArea = new List<IPickupable<IThrowable>>();
        public List<IPickupable<IThrowable>> PickablesInArea => m_PickablesInArea;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent<IPickupable<IThrowable>>(out var pickupable))
            {
                m_PickablesInArea.Add(pickupable);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.TryGetComponent<IPickupable<IThrowable>>(out var pickupable))
            {
                m_PickablesInArea.Remove(pickupable);
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.collider.TryGetComponent<IPickupable<IThrowable>>(out var pickupable))
            {
                m_PickablesInArea.Add(pickupable);
            }
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if (collision.collider.TryGetComponent<IPickupable<IThrowable>>(out var pickupable))
            {
                m_PickablesInArea.Remove(pickupable);
            }
        }


    }
}
