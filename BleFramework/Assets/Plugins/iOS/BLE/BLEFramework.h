#import <Foundation/Foundation.h>


extern NSString *const BLEUnityMessageName_OnBleDidInitialize;
extern NSString *const BLEUnityMessageName_OnBleDidCompletePeripheralScan;
extern NSString *const BLEUnityMessageName_OnBleDidConnect;
extern NSString *const BLEUnityMessageName_OnBleDidDisconnect;
extern NSString *const BLEUnityMessageName_OnBleDidReceiveData;

@interface BLEFrameworkDelegate : NSObject

@property (readonly, strong, nonatomic) NSData *dataRx;

@end

