# Firebase Analytics

Two implementation

1. Web
2. App ( ios/Android )

Library Choise

1. Web: ( Firebase npm module - [https://firebase.google.com/docs/analytics/get-started?platform=web](https://firebase.google.com/docs/analytics/get-started?platform=web))
2. App: ( React-native-firebase-module [https://rnfirebase.io/analytics/usage](https://rnfirebase.io/analytics/usage) )

EggShell Project

- Thirdparty
    - GoogleAnalytics
        - Base
        - Web

            Usages - `firebase` npm module

        - Native

            Usages - `React-native-firebase` analytics module


GA events/trackings

1. **Track Screenviews (** [https://firebase.google.com/docs/analytics/screenviews#web-version-9](https://firebase.google.com/docs/analytics/screenviews#web-version-9) )
2. **Set a user ID (** [https://firebase.google.com/docs/analytics/userid](https://firebase.google.com/docs/analytics/userid) ) PS - No web implementation guideline
3. **Measure Ecommerce (** [https://firebase.google.com/docs/analytics/measure-ecommerce](https://firebase.google.com/docs/analytics/measure-ecommerce) )
    1. `view_item_list` 
        1. Web - [https://firebase.google.com/docs/analytics/measure-ecommerce#select_product](https://firebase.google.com/docs/analytics/measure-ecommerce#select_product)
        2. App - [https://rnfirebase.io/reference/analytics#logViewItemList](https://rnfirebase.io/reference/analytics#logViewItemList)
    2. `select_item` ( PS - Might be difficult to implement, low prio )
    3. `view_item` 
        1. Web: [https://firebase.google.com/docs/analytics/measure-ecommerce#view_product](https://firebase.google.com/docs/analytics/measure-ecommerce#view_product) 
        2. App: [https://rnfirebase.io/reference/analytics#logViewItem](https://rnfirebase.io/reference/analytics#logViewItem)
    4. `add_to_cart` 
        1. Web: [https://firebase.google.com/docs/analytics/measure-ecommerce#add_remove_product](https://firebase.google.com/docs/analytics/measure-ecommerce#add_remove_product) 
        2. App: [https://rnfirebase.io/reference/analytics#logAddToCart](https://rnfirebase.io/reference/analytics#logAddToCart)
    5. `view_cart` 
        1. Web (Scroll to `view_cart` section ): [https://firebase.google.com/docs/analytics/measure-ecommerce#add_remove_product](https://firebase.google.com/docs/analytics/measure-ecommerce#add_remove_product) 
        2. App: [analytics | React Native Firebase (rnfirebase.io)](https://rnfirebase.io/reference/analytics#logViewCart)
    6. `remove_from_cart`
        1. Web (Scroll to `remove_from_cart`section ): [https://firebase.google.com/docs/analytics/measure-ecommerce#add_remove_product](https://firebase.google.com/docs/analytics/measure-ecommerce#add_remove_product) 
        2. App: [https://rnfirebase.io/reference/analytics#logRemoveFromCart](https://rnfirebase.io/reference/analytics#logRemoveFromCart)
    7. `begin_checkout`
        1. Web: [https://firebase.google.com/docs/analytics/measure-ecommerce#initiate_checkout](https://firebase.google.com/docs/analytics/measure-ecommerce#initiate_checkout)
        2. App: [https://rnfirebase.io/reference/analytics#logBeginCheckout](https://rnfirebase.io/reference/analytics#logBeginCheckout)
    8. `add_shipping_info`
        1. Web (Scroll to `add_shipping_info` section) :  [https://firebase.google.com/docs/analytics/measure-ecommerce#web-version-9_5](https://firebase.google.com/docs/analytics/measure-ecommerce#web-version-9_5)
        2. App: [https://rnfirebase.io/reference/analytics#logAddShippingInfo](https://rnfirebase.io/reference/analytics#logAddShippingInfo)
    9. `add_payment_info` ( PS - No implementing, as no online payment available currently )
    10. `purchase` 
        1. Web: [https://firebase.google.com/docs/analytics/measure-ecommerce#make_purchase_refund](https://firebase.google.com/docs/analytics/measure-ecommerce#make_purchase_refund) 
        2. App: [https://rnfirebase.io/reference/analytics#logPurchase](https://rnfirebase.io/reference/analytics#logPurchase)
    11. Promotion and refund ignored as these feature are not available