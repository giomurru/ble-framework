#import <Foundation/Foundation.h>



@interface BLEFrameworkDelegate : NSObject

@property (strong,nonatomic) NSMutableArray *mDevices;
@property (nonatomic) BOOL isConnected;
@property (nonatomic) BOOL searchDevicesDidFinish;

- (void)scanForPeripherals;
- (void)connectToDeviceAtIndex:(NSUInteger)index;
- (void)sendDataToPeripheral:(UInt8 *)buf;
+(void)SendUnityMessage:(NSString*)functionName arrayValuesToPass:(NSArray*)arrayValues;
+(void)SendUnityMessage:(NSString*)functionName message:(NSString*)message;

@end

