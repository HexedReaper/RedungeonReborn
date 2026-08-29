using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Knighter.Messages;
using Plugin.InAppBilling;
using Plugin.InAppBilling.Abstractions;

namespace Knighter;

public class Store : Component, IStore
{
	public delegate void OnProductPurchase(Iap iap, bool succeed);

	private List<InAppBillingProduct> products;

	private Regex priceExtracter = new Regex("[0-9]+((\\.|\\,)[0-9]+)+");

	public static readonly Dictionary<Iap, int> CoinsForOffer = new Dictionary<Iap, int>
	{
		{
			Iap.Offer1,
			3000
		},
		{
			Iap.Offer2,
			12000
		},
		{
			Iap.Offer3,
			50000
		}
	};

	private static readonly Dictionary<Iap, string> productIds = new Dictionary<Iap, string>
	{
		{
			Iap.Offer1,
			"com.nitrome.redungeon.smallchest"
		},
		{
			Iap.Offer2,
			"com.nitrome.redungeon.mediumchest"
		},
		{
			Iap.Offer3,
			"com.nitrome.redungeon.bigchest"
		},
		{
			Iap.RemoveAds,
			"com.nitrome.redungeon.removeads"
		},
		{
			Iap.CoinDoubler,
			"com.nitrome.redungeon.coindoubler"
		}
	};

	private readonly Dictionary<Iap, OnProductPurchase> purchaseHandlers;

	public static event EventHandler<Iap> ProductPurchased;

	public static event EventHandler<Iap> ProductPurchaseFailed;

	public static event EventHandler<Iap> ProductRestored;

	public static event EventHandler<List<InAppBillingProduct>> ProductsReceived;

	public void RestorePurchases()
	{
		SendMessage(new CoreEventMessage(CoreEvent.Wait));
		Task.Run((Func<Task>)RestorePurchasesAsync);
	}

	private async Task RestorePurchasesAsync()
	{
		List<InAppBillingPurchase> list = await GetPurchasesAsync();
		if (list != null)
		{
			foreach (InAppBillingPurchase purchase in list)
			{
				if (purchase.ProductId == productIds[Iap.RemoveAds])
				{
					ProductRestored(this, Iap.RemoveAds);
				}
				else if (purchase.ProductId == productIds[Iap.CoinDoubler])
				{
					ProductRestored(this, Iap.CoinDoubler);
				}
				else if (purchase.ConsumptionState == ConsumptionState.NoYetConsumed && await CrossInAppBilling.Current.ConsumePurchaseAsync(purchase.ProductId, purchase.PurchaseToken) != null)
				{
					ProductRestored(this, GetIap(purchase.ProductId));
				}
			}
		}
		SendMessage(new CoreEventMessage(CoreEvent.StopWait));
	}

	private async Task<List<InAppBillingPurchase>> GetPurchasesAsync()
	{
		List<InAppBillingPurchase> purchases = null;
		IInAppBilling billing = CrossInAppBilling.Current;
		try
		{
			_ = 1;
			try
			{
				if (await billing.ConnectAsync())
				{
					purchases = (await billing.GetPurchasesAsync(ItemType.InAppPurchase)).ToList();
				}
			}
			catch (InAppBillingPurchaseException)
			{
			}
			catch (Exception)
			{
			}
		}
		finally
		{
			await billing.DisconnectAsync();
		}
		return purchases;
	}

	private async Task RequestProductsAsync()
	{
		IInAppBilling billing = CrossInAppBilling.Current;
		try
		{
			_ = 1;
			try
			{
				if (await billing.ConnectAsync())
				{
					IEnumerable<InAppBillingProduct> source = await billing.GetProductInfoAsync(ItemType.InAppPurchase, productIds.Values.ToArray());
					ProductsReceived(this, source.ToList());
				}
			}
			catch (InAppBillingPurchaseException)
			{
			}
			catch (Exception)
			{
			}
		}
		finally
		{
			await billing.DisconnectAsync();
		}
	}

	public void RequestProducts()
	{
		Task.Run((Func<Task>)RequestProductsAsync);
	}

	public bool CanMakePayments()
	{
		return true;
	}

	private async Task PurchaseProductAsync(string productId, string payload)
	{
		IInAppBilling billing = CrossInAppBilling.Current;
		try
		{
			_ = 2;
			try
			{
				if (!(await billing.ConnectAsync()))
				{
					return;
				}
				InAppBillingPurchase inAppBillingPurchase = await billing.PurchaseAsync(productId, ItemType.InAppPurchase, payload);
				if (inAppBillingPurchase == null || inAppBillingPurchase.State != PurchaseState.Purchased)
				{
					return;
				}
				if (IsConsumable(inAppBillingPurchase.ProductId))
				{
					if (await billing.ConsumePurchaseAsync(inAppBillingPurchase.ProductId, inAppBillingPurchase.PurchaseToken) != null)
					{
						ProductPurchased(this, GetIap(productId));
					}
					else
					{
						ProductPurchaseFailed(this, GetIap(productId));
					}
				}
				else
				{
					ProductPurchased(this, GetIap(productId));
				}
			}
			catch (InAppBillingPurchaseException ex)
			{
				if (ex.PurchaseError == PurchaseError.AlreadyOwned)
				{
					ProductPurchased(this, GetIap(productId));
				}
				else
				{
					ProductPurchaseFailed(this, GetIap(productId));
				}
			}
			catch (Exception)
			{
				ProductPurchaseFailed(this, GetIap(productId));
			}
		}
		finally
		{
			await billing.DisconnectAsync();
		}
	}

	public bool PurchaseProduct(Iap iap)
	{
		if (!CanMakePayments())
		{
			return false;
		}
		InAppBillingProduct product = GetProduct(iap);
		if (product == null)
		{
			return false;
		}
		Task.Run(() => PurchaseProductAsync(product.ProductId, string.Empty));
		return true;
	}

	public bool AllProductsAvailable()
	{
		if (products != null)
		{
			return products.Count == productIds.Count;
		}
		return false;
	}

	public string GetPrice(Iap iap)
	{
		if (!AllProductsAvailable())
		{
			return "n/a";
		}
		InAppBillingProduct product = GetProduct(iap);
		if (base.core.Renderer.CanDrawText(product.LocalizedPrice))
		{
			return product.LocalizedPrice;
		}
		return priceExtracter.Match(product.LocalizedPrice).Value + " " + product.CurrencyCode;
	}

	private InAppBillingProduct GetProduct(Iap iap)
	{
		if (products == null)
		{
			return null;
		}
		foreach (InAppBillingProduct product in products)
		{
			if (product.ProductId == productIds[iap])
			{
				return product;
			}
		}
		return null;
	}

	private static Iap GetIap(string productId)
	{
		foreach (Iap value in Enum.GetValues(typeof(Iap)))
		{
			if (productIds[value] == productId)
			{
				return value;
			}
		}
		return Iap.Offer1;
	}

	public void Initialize()
	{
		ProductPurchased += delegate(object sender, Iap iap)
		{
			SendMessage(new CoreEventMessage(CoreEvent.StopWait));
			ACTUALIZE_PURCHASE(iap);
			InvokePurchaseHandler(iap, succeed: true);
		};
		ProductPurchaseFailed += delegate(object sender, Iap iap)
		{
			SendMessage(new CoreEventMessage(CoreEvent.StopWait));
			InvokePurchaseHandler(iap, succeed: false);
		};
		ProductRestored += delegate(object sender, Iap iap)
		{
			ACTUALIZE_PURCHASE(iap);
		};
		ProductsReceived += delegate(object sender, List<InAppBillingProduct> items)
		{
			products = items;
			SendMessage(new ReceivedProductsMessage());
		};
	}

	public Store()
	{
		purchaseHandlers = new Dictionary<Iap, OnProductPurchase>();
	}

	private void InvokePurchaseHandler(Iap iap, bool succeed)
	{
		new Task(async delegate
		{
			await Task.Delay(500);
			if (purchaseHandlers.ContainsKey(iap))
			{
				purchaseHandlers[iap](iap, succeed);
				purchaseHandlers.Remove(iap);
			}
		}).Start();
	}

	private void ACTUALIZE_PURCHASE(Iap iap)
	{
		base.core.Analytics.TrackEvent(AnalyticsCategory.Purchase, iap.ToString(), "");
		switch (iap)
		{
		case Iap.Offer1:
		case Iap.Offer2:
		case Iap.Offer3:
			base.core.ProfileData.AddCoins(CoinsForOffer[iap]);
			break;
		case Iap.RemoveAds:
			base.core.ProfileData.RemoveAds();
			break;
		case Iap.CoinDoubler:
			base.core.ProfileData.EnableCoinDoubler();
			break;
		}
		base.core.Cloud.Sync();
	}

	public void PurchaseProduct(Iap iap, OnProductPurchase onProductPurchase)
	{
		purchaseHandlers[iap] = onProductPurchase;
		if (!PurchaseProduct(iap))
		{
			InvokePurchaseHandler(iap, succeed: false);
		}
		else
		{
			SendMessage(new CoreEventMessage(CoreEvent.Wait));
		}
	}

	public bool IsConsumable(Iap iap)
	{
		return IsConsumable(productIds[iap]);
	}

	public bool IsConsumable(string productId)
	{
		if (productId != productIds[Iap.CoinDoubler])
		{
			return productId != productIds[Iap.RemoveAds];
		}
		return false;
	}
}
