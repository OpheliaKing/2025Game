using Fusion;
using UnityEngine;
using System.Collections.Generic;

namespace Shin
{
    /// <summary>
    /// 포톤 퓨전에서 포지션 및 애니메이션 동기화를 위한 예제 스크립트
    /// CharacterBase에 추가하거나 별도 컴포넌트로 사용 가능
    /// </summary>
    public class CharacterPositionSync : NetworkBehaviour
    {
        [Header("Position Sync Settings")]
        [SerializeField] private bool _syncPosition = true;
        [SerializeField] private bool _syncRotation = false;
        [SerializeField] private float _interpolationSpeed = 10f;

        [Header("Animation Sync Settings")]
        [SerializeField] private bool _syncAnimation = true;
        [SerializeField] private string[] _floatParameters = new string[] { "Speed", "VelocityX", "VelocityY" };
        [SerializeField] private string[] _intParameters = new string[] { };
        [SerializeField] private string[] _boolParameters = new string[] { "IsGrounded", "IsMoving" };
        [SerializeField] private string[] _triggerParameters = new string[] { "Jump", "Attack" };

        // Networked 속성으로 포지션 동기화
        [Networked] private Vector3 NetworkedPosition { get; set; }
        [Networked] private Quaternion NetworkedRotation { get; set; }

        // Networked 속성으로 애니메이션 파라미터 동기화
        [Networked, Capacity(10)] private NetworkDictionary<string, float> NetworkedFloatParams => default;
        [Networked, Capacity(10)] private NetworkDictionary<string, int> NetworkedIntParams => default;
        [Networked, Capacity(10)] private NetworkDictionary<string, bool> NetworkedBoolParams => default;
        [Networked, Capacity(10)] private NetworkDictionary<string, bool> NetworkedTriggerParams => default;

        private Rigidbody2D _rb;
        private Transform _transform;
        [SerializeField]
        private Animator _animator;
        private Dictionary<string, float> _lastFloatValues = new Dictionary<string, float>();
        private Dictionary<string, int> _lastIntValues = new Dictionary<string, int>();
        private Dictionary<string, bool> _lastBoolValues = new Dictionary<string, bool>();

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _transform = transform;
            _animator = GetComponentInChildren<Animator>();
        }

        public override void Spawned()
        {
            // 스폰 시 초기 포지션 설정
            if (Object.HasInputAuthority)
            {
                // 내 캐릭터인 경우 현재 포지션을 네트워크 포지션에 저장
                NetworkedPosition = _transform.position;
                NetworkedRotation = _transform.rotation;

                // 애니메이션 파라미터 초기화
                if (_syncAnimation && _animator != null)
                {
                    InitializeAnimationParameters();
                }
            }
        }

        public override void FixedUpdateNetwork()
        {
            if (Object.HasInputAuthority)
            {
                // 내 캐릭터: 현재 포지션을 네트워크에 동기화
                // State Authority를 가진 경우 직접 변경, 아니면 RPC로 요청
                if (Object.HasStateAuthority)
                {
                    // State Authority를 가진 경우 직접 변경 가능
                    NetworkedPosition = _transform.position;
                    if (_syncRotation)
                    {
                        NetworkedRotation = _transform.rotation;
                    }
                }
                else
                {
                    // State Authority가 없는 경우 RPC로 서버에 요청
                    RpcUpdatePosition(_transform.position, _syncRotation ? _transform.rotation : Quaternion.identity);
                }

                // 애니메이션 파라미터 동기화 (로컬 → 네트워크)
                if (_syncAnimation && _animator != null)
                {
                    SyncAnimationToNetwork();
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
                Debug.Log("Test _syncAnimation : " + _syncAnimation);
                Debug.Log("Test _animator : " + _animator);
                // 애니메이션 파라미터 동기화 (네트워크 → 로컬)
                if (_syncAnimation && _animator != null)
                {
                    SyncNetworkToAnimation();
                }
            }
        }


        /// <summary>
        /// 클라이언트가 서버(State Authority)에 포지션 동기화를 요청하는 RPC
        /// Input Authority를 가진 클라이언트가 호출하면, State Authority를 가진 클라이언트에서 실행됩니다.
        /// </summary>
        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        public void RpcUpdatePosition(Vector3 position, Quaternion rotation, RpcInfo info = default)
        {
            // State Authority를 가진 클라이언트에서만 실행됨
            // [Networked] 속성은 State Authority에서만 변경 가능
            NetworkedPosition = position;
            
            if (_syncRotation)
            {
                NetworkedRotation = rotation;
            }
            
            Debug.Log($"[RpcUpdatePosition] 포지션 업데이트: {position}, 요청 플레이어: {info.Source}");
        }
        
        /// <summary>
        /// 클라이언트에서 포지션 동기화를 요청하는 공개 메서드
        /// Input Authority를 가진 클라이언트에서 호출 가능
        /// </summary>
        public void RequestPositionUpdate()
        {
            if (Object.HasInputAuthority)
            {
                RpcUpdatePosition(_transform.position, _syncRotation ? _transform.rotation : Quaternion.identity);
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

        /// <summary>
        /// 애니메이션 파라미터 초기화
        /// </summary>
        private void InitializeAnimationParameters()
        {
            // Float 파라미터 초기화
            foreach (var paramName in _floatParameters)
            {
                if (_animator.parameters != null)
                {
                    foreach (var param in _animator.parameters)
                    {
                        if (param.name == paramName && param.type == AnimatorControllerParameterType.Float)
                        {
                            float value = _animator.GetFloat(paramName);
                            NetworkedFloatParams.Set(paramName, value);
                            _lastFloatValues[paramName] = value;
                            break;
                        }
                    }
                }
            }

            // Int 파라미터 초기화
            foreach (var paramName in _intParameters)
            {
                if (_animator.parameters != null)
                {
                    foreach (var param in _animator.parameters)
                    {
                        if (param.name == paramName && param.type == AnimatorControllerParameterType.Int)
                        {
                            int value = _animator.GetInteger(paramName);
                            NetworkedIntParams.Set(paramName, value);
                            _lastIntValues[paramName] = value;
                            break;
                        }
                    }
                }
            }

            // Bool 파라미터 초기화
            foreach (var paramName in _boolParameters)
            {
                if (_animator.parameters != null)
                {
                    foreach (var param in _animator.parameters)
                    {
                        if (param.name == paramName && param.type == AnimatorControllerParameterType.Bool)
                        {
                            bool value = _animator.GetBool(paramName);
                            NetworkedBoolParams.Set(paramName, value);
                            _lastBoolValues[paramName] = value;
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 로컬 Animator 파라미터를 네트워크로 동기화 (InputAuthority)
        /// </summary>
        private void SyncAnimationToNetwork()
        {
            // Float 파라미터 동기화
            foreach (var paramName in _floatParameters)
            {
                if (_animator.parameters != null)
                {
                    foreach (var param in _animator.parameters)
                    {
                        Debug.Log("Test SyncAnimationToNetwork param: " + param.name);
                        Debug.Log("Test SyncAnimationToNetwork paramName: " + paramName);
                        Debug.Log("Test SyncAnimationToNetwork param.type: " + param.type);
                        if (param.name == paramName && param.type == AnimatorControllerParameterType.Float)
                        {
                            Debug.Log("Test SyncAnimationToNetwork Float: " + paramName);
                            float currentValue = _animator.GetFloat(paramName);
                            if (!_lastFloatValues.ContainsKey(paramName) ||
                                Mathf.Abs(_lastFloatValues[paramName] - currentValue) > 0.01f)
                            {
                                NetworkedFloatParams.Set(paramName, currentValue);
                                _lastFloatValues[paramName] = currentValue;
                            }
                            break;
                        }
                    }
                }
            }

            // Int 파라미터 동기화
            foreach (var paramName in _intParameters)
            {
                if (_animator.parameters != null)
                {
                    foreach (var param in _animator.parameters)
                    {
                        if (param.name == paramName && param.type == AnimatorControllerParameterType.Int)
                        {
                            int currentValue = _animator.GetInteger(paramName);
                            if (!_lastIntValues.ContainsKey(paramName) || _lastIntValues[paramName] != currentValue)
                            {
                                NetworkedIntParams.Set(paramName, currentValue);
                                _lastIntValues[paramName] = currentValue;
                            }
                            break;
                        }
                    }
                }
            }

            // Bool 파라미터 동기화
            foreach (var paramName in _boolParameters)
            {
                if (_animator.parameters != null)
                {
                    foreach (var param in _animator.parameters)
                    {
                        if (param.name == paramName && param.type == AnimatorControllerParameterType.Bool)
                        {
                            bool currentValue = _animator.GetBool(paramName);
                            if (!_lastBoolValues.ContainsKey(paramName) || _lastBoolValues[paramName] != currentValue)
                            {
                                NetworkedBoolParams.Set(paramName, currentValue);
                                _lastBoolValues[paramName] = currentValue;
                            }
                            break;
                        }
                    }
                }
            }

            // Trigger 파라미터는 FixedUpdateNetwork에서 처리하지 않고
            // 별도 메서드를 통해 호출해야 함 (SetNetworkTrigger 사용)
        }

        /// <summary>
        /// 네트워크 파라미터를 로컬 Animator로 동기화 (다른 플레이어)
        /// </summary>
        private void SyncNetworkToAnimation()
        {
            // Float 파라미터 동기화
            foreach (var kvp in NetworkedFloatParams)
            {
                if (_animator.parameters != null)
                {
                    foreach (var param in _animator.parameters)
                    {
                        if (param.name == kvp.Key && param.type == AnimatorControllerParameterType.Float)
                        {
                            Debug.Log("Test SyncNetworkToAnimation Float: " + kvp.Key + " " + kvp.Value);
                            _animator.SetFloat(kvp.Key, kvp.Value);
                            break;
                        }
                    }
                }
            }

            // Int 파라미터 동기화
            foreach (var kvp in NetworkedIntParams)
            {
                if (_animator.parameters != null)
                {
                    foreach (var param in _animator.parameters)
                    {
                        if (param.name == kvp.Key && param.type == AnimatorControllerParameterType.Int)
                        {
                            Debug.Log("Test SyncNetworkToAnimation Int: " + kvp.Key + " " + kvp.Value);
                            _animator.SetInteger(kvp.Key, kvp.Value);
                            break;
                        }
                    }
                }
            }

            // Bool 파라미터 동기화
            foreach (var kvp in NetworkedBoolParams)
            {
                if (_animator.parameters != null)
                {
                    foreach (var param in _animator.parameters)
                    {
                        if (param.name == kvp.Key && param.type == AnimatorControllerParameterType.Bool)
                        {
                            _animator.SetBool(kvp.Key, kvp.Value);
                            break;
                        }
                    }
                }
            }

            // Trigger 파라미터 동기화
            foreach (var kvp in NetworkedTriggerParams)
            {
                if (kvp.Value && _animator.parameters != null)
                {
                    foreach (var param in _animator.parameters)
                    {
                        if (param.name == kvp.Key && param.type == AnimatorControllerParameterType.Trigger)
                        {
                            _animator.SetTrigger(kvp.Key);
                            // Trigger는 한 번만 실행되도록 false로 리셋
                            NetworkedTriggerParams.Set(kvp.Key, false);
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 네트워크로 Trigger 파라미터를 설정하는 공개 메서드
        /// InputAuthority를 가진 플레이어만 호출 가능
        /// </summary>
        public void SetNetworkTrigger(string triggerName)
        {
            if (Object.HasInputAuthority && _syncAnimation && _animator != null)
            {
                // 로컬에서도 즉시 실행
                _animator.SetTrigger(triggerName);
                
                // 네트워크로 동기화
                NetworkedTriggerParams.Set(triggerName, true);
            }
        }

        /// <summary>
        /// 네트워크로 Float 파라미터를 직접 설정하는 공개 메서드
        /// </summary>
        public void SetNetworkFloat(string paramName, float value)
        {
            if (Object.HasInputAuthority && _syncAnimation && _animator != null)
            {
                _animator.SetFloat(paramName, value);
                NetworkedFloatParams.Set(paramName, value);
                _lastFloatValues[paramName] = value;
            }
        }

        /// <summary>
        /// 네트워크로 Int 파라미터를 직접 설정하는 공개 메서드
        /// </summary>
        public void SetNetworkInt(string paramName, int value)
        {
            if (Object.HasInputAuthority && _syncAnimation && _animator != null)
            {
                _animator.SetInteger(paramName, value);
                NetworkedIntParams.Set(paramName, value);
                _lastIntValues[paramName] = value;
            }
        }

        /// <summary>
        /// 네트워크로 Bool 파라미터를 직접 설정하는 공개 메서드
        /// </summary>
        public void SetNetworkBool(string paramName, bool value)
        {
            if (Object.HasInputAuthority && _syncAnimation && _animator != null)
            {
                _animator.SetBool(paramName, value);
                NetworkedBoolParams.Set(paramName, value);
                _lastBoolValues[paramName] = value;
            }
        }

        #region 디버그 헬퍼 메서드

        /// <summary>
        /// InputAuthority가 없는 오브젝트에서만 디버그 로그 출력
        /// </summary>
        private void LogIfNoInputAuthority(string message)
        {
            if (!Object.HasInputAuthority)
            {
                Debug.Log($"[NoInputAuthority] {gameObject.name}: {message}");
            }
        }

        /// <summary>
        /// Proxy 오브젝트에서만 디버그 로그 출력
        /// </summary>
        private void LogIfProxy(string message)
        {
            if (Object.IsProxy)
            {
                Debug.Log($"[Proxy] {gameObject.name}: {message}");
            }
        }

        /// <summary>
        /// State Authority를 가진 클라이언트에서만 디버그 로그 출력
        /// </summary>
        private void LogIfStateAuthority(string message)
        {
            if (Object.HasStateAuthority)
            {
                Debug.Log($"[StateAuthority] {gameObject.name}: {message}");
            }
        }

        /// <summary>
        /// 상세한 네트워크 상태 정보 출력
        /// </summary>
        private void LogNetworkStatus(string context = "")
        {
            if (!Object.HasInputAuthority)
            {
                Debug.Log($"[NetworkStatus] {gameObject.name} {context}\n" +
                    $"  - HasInputAuthority: {Object.HasInputAuthority}\n" +
                    $"  - HasStateAuthority: {Object.HasStateAuthority}\n" +
                    $"  - IsProxy: {Object.IsProxy}\n" +
                    $"  - InputAuthority: {(Object.InputAuthority != null ? Object.InputAuthority.PlayerId.ToString() : "null")}\n" +
                    $"  - Runner.IsServer: {(Runner != null ? Runner.IsServer.ToString() : "null")}\n" +
                    $"  - Runner.IsClient: {(Runner != null ? Runner.IsClient.ToString() : "null")}");
            }
        }

        #endregion
    }
}

