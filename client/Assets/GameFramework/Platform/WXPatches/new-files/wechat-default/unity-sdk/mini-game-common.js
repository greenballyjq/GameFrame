import { formatJsonStr } from './utils';

const MGC_MODULE = 'WXMiniGameCommon';

function sendToMGC(method, str = '') {
  if (GameGlobal.Module && GameGlobal.Module.SendMessage) {
    GameGlobal.Module.SendMessage(MGC_MODULE, method, typeof str === 'string' ? str : String(str));
  }
}

let sdkInstance = null;

function getSDKInstance() {
  if (!sdkInstance) {
    sdkInstance = GameGlobal.miniGameCommon || null;
  }
  return sdkInstance;
}

export default {
  // ==================== 检查插件是否可用 ====================

  WXMiniGameCommonIsAvailable() {
    return !!getSDKInstance();
  },

  WXMiniGameCommonCanIUse(feature) {
    const sdk = getSDKInstance();
    if (sdk && sdk.canIUse) {
      return sdk.canIUse(feature);
    }
    return false;
  },

  // ==================== 分享图片到游戏圈 ====================

  // 分享CDN图片到游戏圈
  // conf: { url, title, content, callbackId }
  WXShareImageToGameClub(conf) {
    const config = formatJsonStr(conf);
    const { url, title, content, callbackId } = config;
    const sdk = getSDKInstance();

    if (!sdk || !sdk.canIUse('shareImageToGameCenter')) {
      sendToMGC('OnShareToGameClubCallback', JSON.stringify({
        callbackId,
        errMsg: 'shareImageToGameCenter not supported',
      }));
      return;
    }

    wx.showLoading({ title: '正在加载...' });
    wx.downloadFile({
      url,
      success: (res) => {
        wx.hideLoading();
        if (res.statusCode === 200) {
          const gameClub = sdk.createGameClub();
          gameClub.shareImageToGameCenter({
            path: res.tempFilePath,
            title: title || '',
            content: content || '',
          }).then(() => {
            sendToMGC('OnShareToGameClubCallback', JSON.stringify({
              callbackId,
              errMsg: 'ok',
            }));
          }).catch((err) => {
            sendToMGC('OnShareToGameClubCallback', JSON.stringify({
              callbackId,
              errMsg: err.errMsg || String(err),
            }));
          });
        } else {
          sendToMGC('OnShareToGameClubCallback', JSON.stringify({
            callbackId,
            errMsg: `downloadFile statusCode: ${res.statusCode}`,
          }));
        }
      },
      fail: (err) => {
        wx.hideLoading();
        sendToMGC('OnShareToGameClubCallback', JSON.stringify({
          callbackId,
          errMsg: err.errMsg || 'downloadFile fail',
        }));
      },
    });
  },

  // 分享canvas截图到游戏圈
  // conf: { x, y, width, height, destWidth, destHeight, title, content, callbackId }
  WXShareCanvasToGameClub(conf) {
    const config = formatJsonStr(conf);
    const { title, content, callbackId } = config;
    const sdk = getSDKInstance();

    if (!sdk || !sdk.canIUse('shareImageToGameCenter')) {
      sendToMGC('OnShareToGameClubCallback', JSON.stringify({
        callbackId,
        errMsg: 'shareImageToGameCenter not supported',
      }));
      return;
    }

    const info = wx.getSystemInfoSync();
    const { pixelRatio, screenWidth, screenHeight } = info;
    const x = config.x || 0;
    const y = config.y || 0;
    const width = config.width || screenWidth * pixelRatio;
    const height = config.height || screenHeight * pixelRatio;
    const destWidth = config.destWidth || screenWidth;
    const destHeight = config.destHeight || screenHeight;

    canvas.toTempFilePath({
      x, y, width, height, destWidth, destHeight,
      success: (res) => {
        const gameClub = sdk.createGameClub();
        gameClub.shareImageToGameCenter({
          path: res.tempFilePath,
          title: title || '',
          content: content || '',
        }).then(() => {
          sendToMGC('OnShareToGameClubCallback', JSON.stringify({
            callbackId,
            errMsg: 'ok',
          }));
        }).catch((err) => {
          sendToMGC('OnShareToGameClubCallback', JSON.stringify({
            callbackId,
            errMsg: err.errMsg || String(err),
          }));
        });
      },
      fail: (err) => {
        sendToMGC('OnShareToGameClubCallback', JSON.stringify({
          callbackId,
          errMsg: err.errMsg || 'toTempFilePath fail',
        }));
      },
    });
  },

  // ==================== 推送小游戏动态 ====================

  // 初始化 GameTimeLine 并监听事件
  WXInitGameTimeLine() {
    const sdk = getSDKInstance();
    if (!sdk) {
      console.error('MiniGameCommon plugin not available');
      return;
    }

    const gameTimeLine = sdk.createGameTimeLine();
    GameGlobal._gameTimeLine = gameTimeLine;

    gameTimeLine.on('error', (err) => {
      sendToMGC('OnGameTimeLineError', JSON.stringify({
        errMsg: err.message || String(err),
      }));
    });

    gameTimeLine.on('ready', () => {
      sendToMGC('OnGameTimeLineReady', '');
    });
  },

  // 获取授权
  WXGameTimeLineAuthorize() {
    const tl = GameGlobal._gameTimeLine;
    if (tl) {
      tl.authorize();
    }
  },

  // 是否已授权
  WXGameTimeLineIsAuthorized() {
    const tl = GameGlobal._gameTimeLine;
    return tl ? !!tl.authorized : false;
  },

  // 推送动态
  // conf: { eventId, query, callbackId }
  WXGameTimeLinePushFeed(conf) {
    const config = formatJsonStr(conf);
    const { eventId, query, callbackId } = config;
    const tl = GameGlobal._gameTimeLine;

    if (!tl) {
      sendToMGC('OnGameTimeLinePushFeedCallback', JSON.stringify({
        callbackId,
        errMsg: 'GameTimeLine not initialized',
      }));
      return;
    }

    tl.pushFeed({
      eventId: parseInt(eventId, 10),
      query: query || '',
      success: () => {
        sendToMGC('OnGameTimeLinePushFeedCallback', JSON.stringify({
          callbackId,
          errMsg: 'ok',
        }));
      },
      fail: (e) => {
        sendToMGC('OnGameTimeLinePushFeedCallback', JSON.stringify({
          callbackId,
          errMsg: e.errMsg || String(e),
        }));
      },
    });
  },
};
