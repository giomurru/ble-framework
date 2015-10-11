#import "BLE.h"
#import "BLEFramework.h"

@interface BLEFrameworkDelegate() <BLEDelegate>
//@property (nonatomic, strong) NSString *rssiValue;
@end

@implementation BLEFrameworkDelegate
{
    BLE *ble;
    //NSTimer *rssiTimer;
}

- (id) init
{
    self = [super init];
    if (self)
    {
        ble = [[BLE alloc] init];
        [ble controlSetup];
        ble.delegate = self;
        self.mDevices = [[NSMutableArray alloc] init];
    }
    
    return self;
}



#pragma mark BLEDelegate methods

-(void) bleDidConnect
{
    NSLog(@"->Connected");
    self.isConnected = true;
    
    // send reset
    UInt8 buf[] = {0x04, 0x00, 0x00};
    NSData *data = [[NSData alloc] initWithBytes:buf length:3];
    [ble write:data];
    
    // Schedule to read RSSI every 1 sec.
    //rssiTimer = [NSTimer scheduledTimerWithTimeInterval:(float)1.0 target:self selector:@selector(readRSSITimer:) userInfo:nil repeats:YES];
}

- (void)bleDidDisconnect
{
    self.isConnected = false;
    
    //[rssiTimer invalidate];
}

-(void)bleDidUpdateRSSI:(NSNumber *) rssi
{
    //self.rssiValue = rssi.stringValue;
}

-(void) bleDidReceiveData:(unsigned char *)data length:(int)length
{
    NSLog(@"Length: %d", length);
    
    // parse data, all commands are in 3-byte
    for (int i = 0; i < length; i+=3)
    {
        NSLog(@"0x%02X, 0x%02X, 0x%02X", data[i], data[i+1], data[i+2]);
    }
}

#pragma mark Instance Methodss
/*
 -(void) readRSSITimer:(NSTimer *)timer
 {
 [ble readRSSI];
 }
 */


- (void)scanForPeripherals
{
    
    if (ble.peripherals)
    {
        ble.peripherals = nil;
    }
    
    [ble findBLEPeripherals:2];
    
    [NSTimer scheduledTimerWithTimeInterval:(float)2.0 target:self selector:@selector(connectionTimer:) userInfo:nil repeats:NO];
}

-(void) connectionTimer:(NSTimer *)timer
{
    if (ble.peripherals.count > 0)
    {
        [self.mDevices removeAllObjects];
        
        int i;
        for (i = 0; i < ble.peripherals.count; i++)
        {
            CBPeripheral *p = [ble.peripherals objectAtIndex:i];
            
            if (p.identifier.UUIDString != NULL)
            {
                [self.mDevices insertObject:p.identifier.UUIDString atIndex:i];
            }
            else
            {
                [self.mDevices insertObject:@"UUID is NULL" atIndex:i];
            }
        }
    }
    else
    {
        NSLog(@"No peripherals found");
        [self.mDevices removeAllObjects];
        
    }
    self.searchDevicesDidFinish = true;
}

- (void)connectToDeviceAtIndex:(NSUInteger)index
{
    [ble connectPeripheral:[ble.peripherals objectAtIndex:index]];
}

- (void)sendDataToPeripheral:(UInt8 *)buf
{
    //UInt8 buf[3] = {0x01, 0x00, 0x00};
    
    NSData *data = [[NSData alloc] initWithBytes:buf length:3];
    [ble write:data];
}

#pragma mark Class Methods

+(void)SendUnityMessage:(NSString*)functionName arrayValuesToPass:(NSArray*)arrayValues
{
#ifdef UNITY_VERSION
    
    NSError *error;
    NSData *arrayData = [NSJSONSerialization dataWithJSONObject:arrayValues options:NSJSONWritingPrettyPrinted error:&error];
    NSString* jsonArrayValues = [NSString stringWithCString:(const char *)[arrayData bytes] encoding:NSUTF8StringEncoding];
    
    UnitySendMessage(strdup([@"BLEController" UTF8String]), strdup([functionName UTF8String]), strdup([jsonArrayValues UTF8String]));
    
#endif
}

+(void)SendUnityMessage:(NSString*)functionName message:(NSString*)message
{
    
#ifdef UNITY_VERSION
    UnitySendMessage(strdup([@"BLEController" UTF8String]), strdup([functionName UTF8String]), strdup([message UTF8String]));
    
#endif
}

@end


static BLEFrameworkDelegate* delegateObject = nil;

// Converts C style string to NSString
NSString* CreateNSString (const char* string)
{
    if (string)
        return [NSString stringWithUTF8String: string];
    else
        return [NSString stringWithUTF8String: ""];
}

// Helper method to create C string copy
char* MakeStringCopy (const char* string)
{
    if (string == NULL)
        return NULL;
    
    char* res = (char*)malloc(strlen(string) + 1);
    strcpy(res, string);
    return res;
}

// When native code plugin is implemented in .mm / .cpp file, then functions
// should be surrounded with extern "C" block to conform C function naming rules
extern "C" {
    
    
    void _InitBLEFramework ()
    {
        delegateObject = [[BLEFrameworkDelegate alloc] init];
    }
    
    void _ScanForPeripherals ()
    {
        [delegateObject scanForPeripherals];
    }
    
    bool _IsDeviceConnected()
    {
        return [delegateObject isConnected];
    }
    
    bool _SearchDevicesDidFinish()
    {
        return [delegateObject searchDevicesDidFinish];
    }
    
    const char* _GetListOfDevices ()
    {
        if ([delegateObject searchDevicesDidFinish])
        {
            NSError *error;
            NSDictionary *jsonObjectToSerialize = [NSDictionary dictionaryWithObject:[delegateObject mDevices] forKey:@"DeviceIDS"];
            NSData *dictionaryData = [NSJSONSerialization dataWithJSONObject:jsonObjectToSerialize options:NSJSONWritingPrettyPrinted error:&error];
            NSString* jsonArrayValues = [NSString stringWithCString:(const char *)[dictionaryData bytes] encoding:NSUTF8StringEncoding];
            
            return MakeStringCopy([jsonArrayValues UTF8String]);
        }
        
        return NULL;
    }
    
    void _ConnectToDevice(int device)
    {
        [delegateObject connectToDeviceAtIndex:(NSUInteger)device];
    }
    
    void _SendData (char *buffer)
    {
        [delegateObject sendDataToPeripheral:(UInt8 *)buffer];
    }
    
}



