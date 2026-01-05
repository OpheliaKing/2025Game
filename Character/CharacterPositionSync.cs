using Fusion;
using UnityEngine;

namespace Shin
{
    /// <summary>
    /// 포톤 퓨전에서 포지션 동기화를 위한 예제 스크립트
    /// CharacterBase에 추가하거나 별도 컴포넌트로 사용 가능
    /// </summary>
    public class CharacterPositionSync : NetworkBehaviour
    {
        [Header("Position Sync Settings")]
        [SerializeField] private bool _syncPosition = true;
        [SerializeField] private bool _syncRotation = false;
        [SerializeField] private float _interpolationSpeed = 10f;

        // Networked 속성으로 포지션 동기화
        [Networked] private Vector3 NetworkedPosition { get; set; }
        [Networked] private Quaternion NetworkedRotation { get; set; }

        private Rigidbody2D _rb;
        private Transform _transform;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _transform = transform;
        }

        public override void Spawned()
        {
            // 스폰 시 초기 포지션 설정
            if (Object.HasInputAuthority)
            {
                // 내 캐릭터인 경우 현재 포지션을 네트워크 포지션에 저장
                NetworkedPosition = _transform.position;
                NetworkedRotation = _transform.rotation;
            }
        }

        public override void FixedUpdateNetwork()
        {
            // InputAuthority를 가진 플레이어만 포지션 업데이트
            if (Object.HasInputAuthority)
            {
                // 내 캐릭터: 현재 포지션을 네트워크에 동기화
                NetworkedPosition = _transform.position;
                if (_syncRotation)
                {
                    NetworkedRotation = _transform.rotation;
                }
            }
            else
            {
                // 다른 플레이어의 캐릭터: 네트워크 포지션으로 동기화
                if (_syncPosition)
                {
                    // Rigidbody2D를 사용하는 경우
                    if (_rb != null)
                    {
                        // Rigidbody2D의 포지션을 직접 설정 (물리 시뮬레이션 중단)
                        _rb.MovePosition(NetworkedPosition);
                    }
                    else
                    {
                        // Rigidbody2D가 없는 경우 Transform 직접 설정
                        _transform.position = NetworkedPosition;
                    }
                }

                if (_syncRotation)
                {
                    if (_rb != null)
                    {
                        _rb.MoveRotation(NetworkedRotation.eulerAngles.z);
                    }
                    else
                    {
                        _transform.rotation = NetworkedRotation;
                    }
                }
            }
        }

        /// <summary>
        /// 부드러운 보간을 위한 렌더 업데이트 (선택사항)
        /// </summary>
        public override void Render()
        {
            // InputAuthority가 없는 경우에만 보간 적용
            if (!Object.HasInputAuthority && _syncPosition)
            {
                // 부드러운 보간을 위해 Lerp 사용
                _transform.position = Vector3.Lerp(
                    _transform.position,
                    NetworkedPosition,
                    Time.deltaTime * _interpolationSpeed
                );
            }
        }
    }
}

