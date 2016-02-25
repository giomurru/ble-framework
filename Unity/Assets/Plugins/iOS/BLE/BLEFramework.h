#import <Foundation/Foundation.h>


extern NSString *const BLEUnityMessageName_OnBleDidInitialize;
extern NSString *const BLEUnityMessageName_OnBleDidCompletePeripheralScan;
extern NSString *const BLEUnityMessageName_OnBleDidConnect;
extern NSString *const BLEUnityMessageName_OnBleDidDisconnect;
extern NSString *const BLEUnityMessageName_OnBleDidReceiveData;

@interface BLEFrameworkDelegate : NSObject

@property (strong,nonatomic) NSMutableArray *mDevices;
@property (nonatomic) BOOL isConnected;
@property (nonatomic) BOOL searchDevicesDidFinish;

- (void)scanForPeripherals;

- (bool)connectPeripheral:(NSString *)peripheralID;
- (bool)connectPeripheralAtIndex:(NSInteger)index;
- (void)sendDataToPeripheral:(UInt8 *)buf;

+ (void)SendUnityMessage:(NSString*)functionName arrayValuesToPass:(NSArray*)arrayValues;
+ (void)SendUnityMessage:(NSString*)functionName message:(NSString*)message;
/* PROPERTIES AND METHODS OF THE IOS BLE FRAMEWORK
@property (strong, nonatomic) NSMutableArray *peripherals;
@property (strong, nonatomic) CBCentralManager *CM;
@property (strong, nonatomic) CBPeripheral *activePeripheral;

-(void) enableReadNotification:(CBPeripheral *)p;
-(void) read;
-(void) writeValue:(CBUUID *)serviceUUID characteristicUUID:(CBUUID *)characteristicUUID p:(CBPeripheral *)p data:(NSData *)data;

-(BOOL) isConnected;
-(void) write:(NSData *)d;
-(void) readRSSI;

-(void) controlSetup;
-(int) findBLEPeripherals:(int) timeout;
-(void) connectPeripheral:(CBPeripheral *)peripheral;

-(UInt16) swap:(UInt16) s;
-(const char *) centralManagerStateToString:(int)state;
-(void) scanTimer:(NSTimer *)timer;
-(void) printKnownPeripherals;
-(void) printPeripheralInfo:(CBPeripheral*)peripheral;

-(void) getAllServicesFromPeripheral:(CBPeripheral *)p;
-(void) getAllCharacteristicsFromPeripheral:(CBPeripheral *)p;
-(CBService *) findServiceFromUUID:(CBUUID *)UUID p:(CBPeripheral *)p;
-(CBCharacteristic *) findCharacteristicFromUUID:(CBUUID *)UUID service:(CBService*)service;

//-(NSString *) NSUUIDToString:(NSUUID *) UUID;
-(NSString *) CBUUIDToString:(CBUUID *) UUID;

-(int) compareCBUUID:(CBUUID *) UUID1 UUID2:(CBUUID *)UUID2;
-(int) compareCBUUIDToInt:(CBUUID *) UUID1 UUID2:(UInt16)UUID2;
-(UInt16) CBUUIDToInt:(CBUUID *) UUID;
-(BOOL) UUIDSAreEqual:(NSUUID *)UUID1 UUID2:(NSUUID *)UUID2;
*/
@end

