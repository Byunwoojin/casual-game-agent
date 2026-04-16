using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

namespace PuzzlePals.Systems
{
    /// <summary>
    /// Unity IAP — 힌트 3종 상품 (5/20/Unlimited).
    /// Sandbox 테스트: Google/Apple 콘솔에 동일 Product ID 등록 후 테스트 계정으로 구매.
    /// 구매 성공 시 HintManager 에 자동 적립.
    /// </summary>
    [DisallowMultipleComponent]
    public class IAPManager : MonoBehaviour, IDetailedStoreListener
    {
        public const string ProductHint5         = "puzzlepals_hint_5";
        public const string ProductHint20        = "puzzlepals_hint_20";
        public const string ProductHintUnlimited = "puzzlepals_hint_unlimited";

        private static IAPManager _instance;

        public static IAPManager Instance
        {
            get
            {
                if (_instance != null) return _instance;
                if (!Application.isPlaying) return null;

                var existing = FindAnyObjectByType<IAPManager>(FindObjectsInactive.Include);
                if (existing != null) { _instance = existing; return _instance; }

                var go = new GameObject(nameof(IAPManager));
                _instance = go.AddComponent<IAPManager>();
                return _instance;
            }
        }

        public bool IsInitialized => _store != null;

        private IStoreController   _store;
        private IExtensionProvider _extensions;

        private void Awake()
        {
            if (_instance != null && _instance != this) { Destroy(gameObject); return; }
            _instance = this;
            DontDestroyOnLoad(gameObject);

            Initialize();
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }

        private void Initialize()
        {
            if (IsInitialized) return;

            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
            builder.AddProduct(ProductHint5,         ProductType.Consumable);
            builder.AddProduct(ProductHint20,        ProductType.Consumable);
            builder.AddProduct(ProductHintUnlimited, ProductType.NonConsumable);

            UnityPurchasing.Initialize(this, builder);
        }

        public void Buy(string productId)
        {
            if (!IsInitialized)
            {
                Debug.LogWarning("[IAPManager] 아직 초기화 전 — 구매 시도 무시");
                return;
            }

            var product = _store.products.WithID(productId);
            if (product == null || !product.availableToPurchase)
            {
                Debug.LogWarning($"[IAPManager] {productId} 구매 불가 (스토어 등록 미완료?)");
                return;
            }

            _store.InitiatePurchase(product);
        }

        // ── IStoreListener ───────────────────────────────

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            _store      = controller;
            _extensions = extensions;
            Debug.Log("[IAPManager] 초기화 완료");
        }

        public void OnInitializeFailed(InitializationFailureReason error)
            => Debug.LogError($"[IAPManager] 초기화 실패: {error}");

        public void OnInitializeFailed(InitializationFailureReason error, string message)
            => Debug.LogError($"[IAPManager] 초기화 실패: {error} — {message}");

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e)
        {
            string id = e.purchasedProduct.definition.id;
            var hint  = FindAnyObjectByType<HintManager>(FindObjectsInactive.Include);

            if (hint == null)
            {
                Debug.LogWarning($"[IAPManager] HintManager 없음 — {id} 구매는 처리되었으나 힌트 적립 실패");
                return PurchaseProcessingResult.Complete;
            }

            switch (id)
            {
                case ProductHint5:         hint.AddPurchasedHints(5);  break;
                case ProductHint20:        hint.AddPurchasedHints(20); break;
                case ProductHintUnlimited: hint.SetUnlimited();        break;
                default: Debug.LogWarning($"[IAPManager] 알 수 없는 상품 ID: {id}"); break;
            }

            Debug.Log($"[IAPManager] 구매 완료: {id}");
            return PurchaseProcessingResult.Complete;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
            => Debug.LogWarning($"[IAPManager] 구매 실패 {product.definition.id}: {failureReason}");

        // IDetailedStoreListener 추가 메서드 (5.x 에서 권장 — 더 풍부한 에러 정보)
        public void OnPurchaseFailed(Product product, PurchaseFailureDescription description)
            => Debug.LogWarning($"[IAPManager] 구매 실패 {product.definition.id}: {description.reason} — {description.message}");
    }
}
