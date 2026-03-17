    WXSetAdServerSideVerificationData(adUnitId, conf) {
        const key = adsByUnitId[adUnitId];
        if (!key || !ads[key]) {
            console.warn('[WX] Ad not found for adUnitId:', adUnitId);
            return;
        }
        const ad = ads[key];
        if (ad.setServerSideVerificationData) {
            const config = formatJsonStr(conf);
            ad.setServerSideVerificationData(config);
        } else {
            console.warn('[WX] setServerSideVerificationData not supported, check base library version >= 3.10.3');
        }
    },