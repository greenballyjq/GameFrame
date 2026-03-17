mergeInto(LibraryManager.library, {
  WXMiniGameCommonIsAvailable: function() {
    return window.WXWASMSDK.WXMiniGameCommonIsAvailable();
  },

  WXMiniGameCommonCanIUse: function(feature) {
    return window.WXWASMSDK.WXMiniGameCommonCanIUse(_WXPointer_stringify_adaptor(feature));
  },

  // 分享到游戏圈
  WXShareImageToGameClub: function(conf) {
    window.WXWASMSDK.WXShareImageToGameClub(_WXPointer_stringify_adaptor(conf));
  },

  WXShareCanvasToGameClub: function(conf) {
    window.WXWASMSDK.WXShareCanvasToGameClub(_WXPointer_stringify_adaptor(conf));
  },

  // 推送动�����
  WXInitGameTimeLine: function() {
    window.WXWASMSDK.WXInitGameTimeLine();
  },

  WXGameTimeLineAuthorize: function() {
    window.WXWASMSDK.WXGameTimeLineAuthorize();
  },

  WXGameTimeLineIsAuthorized: function() {
    return window.WXWASMSDK.WXGameTimeLineIsAuthorized();
  },

  WXGameTimeLinePushFeed: function(conf) {
    window.WXWASMSDK.WXGameTimeLinePushFeed(_WXPointer_stringify_adaptor(conf));
  },
});
