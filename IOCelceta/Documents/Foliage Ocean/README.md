# Foliage Ocean

An IO List framework for Mitsubishi QCPU & RCPU.

## Table of contents

- [License](#license)
- [Introduction](#introduction)

- [Metadata](#metadata)
  - [Controller Model Catalogue](#controller-model-catalogue)
  - [Data Type Catalogue](#data-type-catalogue)
  - [Variable Catalogue](#variable-catalogue)
- [IO List](#io-list)
  - [TargetInfo](#targetinfo)
  - [ControllerInfo](#controllerinfo)
  - [Objects](#objects)
  - [RxPDO/TxPDO](#rxpdotxpdo)
  - [Interlocks](#interlocks)
- [Revision](#revision)



## License

TBD



## Introduction

The Foliage Ocean IO List provides an abstraction layer between software and hardware, it can help to reduce coupling degree between software code and hardware IO modules, also achieve code reusability.

The framework assumes nothing about metadata, all metadata can be defined by user. User could adopt entirely different metadata for different control system.

All metadata files and IO list file of a specific control system are xml file based implementations. 

Any subtle change in these files can be found out easily with the help of a text comparing tool(eg. VS code).

All the files have rigid predefined-format to follow and restrictions to comply with, in other words, creator or editor must input content in accordance with the predefined-format and also be subject to restrictions. But do not worry too much about the issue, a GUI tool has been developed to help you to create or edit these files. Please refer to ***IO Celceta*** documents for details. 



## Metadata

### Controller Model Catalogue

**File name**

*controller_model_catalogue.xml*



**Syntax**

```xml
<?xml version="1.0" encoding="utf-8"?>
<AMECControllerModels FormatVersion="1">
    <ExtensionModels>
        <ExtensionModel>
            <ID>0x000F</ID>
            <Name>RY41PT1P</Name>
            <BitSize>0x20</BitSize>
            <RX>
                <Bit>32</Bit>
            </RX>
            <TX>
                <Bit>32</Bit>
            </TX>
        </ExtensionModel>
        <ExtensionModel>
            <ID>0x0050</ID>
            <Name>R60TCRT4</Name>
            <BitSize>0x10</BitSize>
            <TX>
                <PV>4</PV>
                <MVh>4</MVh>
                <MVc>4</MVc>
                <SV>4</SV>
            </TX>
            <RX>
                <SV>4</SV>
                <ManualMV>4</ManualMV>
                <AnalogPV>4</AnalogPV>
            </RX>
        </ExtensionModel>
    </ExtensionModels>
    <EthernetModels>
        <EthernetModel>
            <ID>0x2001</ID>
            <Name>PC104_SERIAL_PORT_SERVER</Name>
            <TX>
                <Raw>2048</Raw>
                <NodeCFG>32</NodeCFG>
                <NodeIOCOM>32</NodeIOCOM>
            </TX>
            <RX>
                <Raw>2048</Raw>
            </RX>
        </EthernetModel>
    </EthernetModels>
</AMECControllerModels>
```



**Elements**

- `/AMECControllerModels`

  The name of root element must be `"AMECControllerModels"`, it must have two child elements,  `ExtensionModels`, `EthernetModels`. 



- `/AMECControllerModels/attribute::FormatVersion`

  The mandatory attribute is used to mark the file format version. The value must be 32-bit unsigned decimal integral number literal. 



- `/AMECControllerModels/ExtensionModels`

  Child element of `AMECControllerModels` , as well as the parent element of `ExtensionModel`. It could contain any number of child `ExtensionModel` elements.



- `/AMECControllerModels/ExtensionModels/ExtensionModel`

  Child element of `ExtensionModels`, the element contains the Extension Model Information. The number of `ExtensionModel`  is unlimited.

  

- `/AMECControllerModels/ExtensionModels/ExtensionModel/ID`

  Extension model ID.  The element text must be 16-bit unsigned hexadecimal integral number literal (begins with 0x). All extension models must have different `ID`.



- `/AMECControllerModels/ExtensionModels/ExtensionModel/Name`

  Extension model name. 



- `/AMECControllerModels/ExtensionModels/ExtensionModel/BitSize`

  The IO addressing space size in bit occupied by extension model.  The element text must be 16-bit unsigned hexadecimal integral number literal (begins with 0x). 



- `/AMECControllerModels/ExtensionModels/ExtensionModel/Tx`

  The child elements are all input data channels of this extension model. The number of data channels is unlimited, but each channel (child element) must be defined as follows:

  ```xml
  <channel_name>the_capcity_of_this_channel</channel_name>
  ```

  The capacity of channel must be 32-bit unsigned decimal integral number literal. The channel name can be arbitrary string.



- `/AMECControllerModels/ExtensionModels/ExtensionModel/Rx`

  The child elements are all output data channels of this extension model. The number of data channels is unlimited, but each channel (child element) must be defined as follows:

  ```xml
  <channel_name>the_capcity_of_this_channel</channel_name>
  ```

  Element name is data channel name and can be  arbitrary string, element text is the capacity of data channel and must be 32-bit unsigned decimal integral number literal. 

  

- `/AMECControllerModels/EthernetModels`

  Child element of `AMECControllerModels` , as well as the parent element of `EthernetModel`. It could contain any number of child `EthernetModel` elements.



- `/AMECControllerModels/EthernetModels/EthernetModel`

  Child element of `EthernetModels`, the element contains the Ethernet Model Information. The number of `EthernetModel`  is unlimited.

  

- `/AMECControllerModels/EthernetModels/EthernetModel/ID`

  Ethernet model ID.  The element text must be 16-bit unsigned hexadecimal integral number literal (begins with 0x). All ethernet models must have different `ID`.



- `/AMECControllerModels/EthernetModels/EthernetModel/Name`

  Ethernet model name. 



- `/AMECControllerModels/EthernetModels/EthernetModel/Tx`

  The child elements are all input data channels of this ethernet model. The number of data channels is unlimited, but each channel (child element) must be defined as follows:

  ```xml
  <channel_name>the_capcity_of_this_channel</channel_name>
  ```

  The capacity of channel must be 32-bit unsigned decimal integral number literal. The channel name can be arbitrary string.



- `/AMECControllerModels/EthernetModels/EthernetModel/Rx`

  The child elements are all output data channels of this ethernet model. The number of data channels is unlimited, but each channel (child element) must be defined as follows:

  ```xml
  <channel_name>the_capcity_of_this_channel</channel_name>
  ```

  The capacity of channel must be 32-bit unsigned decimal integral number literal. The channel name can be arbitrary string.



### Data Type Catalogue

**File name**

*data_type_catalogue.xml*



**Syntax**

```xml
<?xml version="1.0" encoding="UTF-8"?>
<AMECDataTypes FormatVersion="1">
    <DataType>
        <Name>BIT</Name>
        <BitSize>1</BitSize>
        <Alignment>1</Alignment>
    </DataType>
    <DataType>
        <Name>INT</Name>
        <BitSize>32</BitSize>
        <Alignment>4</Alignment>
    </DataType>
    <DataType>
        <Name>DEVICENET_MFC_DEFAULT_TX</Name>
        <BitSize>48</BitSize>
        <Alignment>4</Alignment>
        <SubItems>
            <SubItem>
                <Name>BYTE</Name>
                <ByteOffset>0</ByteOffset>
                <Comment>ExceptionStatus</Comment>
            </SubItem>
            <SubItem>
                <Name>FLOAT</Name>
                <ByteOffset>1</ByteOffset>
                <Comment>Flow(sccm)</Comment>
            </SubItem>
            <SubItem>
                <Name>BYTE</Name>
                <ByteOffset>5</ByteOffset>
                <Comment>Dummy</Comment>
            </SubItem>
        </SubItems>
    </DataType>
</AMECDataTypes>
```

**Elements**

- `/AMECDataTypes`

  The name of root element must be `"AMECDataTypes"`, The element is parent element of `DataType`. It could contain any number of child `DataType` elements.



- `/AMECDataTypes/attribute::FormatVersion`

  The mandatory attribute is used to mark the file format version. The value must be 32-bit unsigned decimal integral number literal. 



- `/AMECDataTypes/DataType`

  Child element of `AMECDataTypes`, the element contains the Data Type Information. The number of `DataType`  is unlimited.



- `/AMECDataTypes/DataType/Name`

  Data type name. All data types must have different `Name`. The data type `Name` text can be arbitrary string. Others can reference this data type using this string.



- `/AMECDataTypes/DataType/BitSize`

  Data type name size in bit. The element text must be 32-bit unsigned decimal integral number literal.



- `/AMECDataTypes/DataType/Alignment`

  Data type alignment boundary, in byte. The element text must be 32-bit unsigned decimal integral number literal and its value must be a power of two.



- `/AMECDataTypes/DataType/SubItems`

  This element is optional. The element is parent element of `SubItem`. It could contain any number of child `SubItem` elements.

  If the element exists, the `BitSize` of data type must be a multiple of 8.

  

- `/AMECDataTypes/DataType/SubItems/SubItem`

  `SubItem` is a component of its parent `DataType`, its alignment boundary is overridden by a fixed value of one byte. The layout of`SubItem` should always be within the bound of its parent `DataType`.



- `/AMECDataTypes/DataType/SubItems/SubItem/Name`

  The data type name of `SubItem`. The data type of the same `Name` must be defined before its parent `DataType`.



- `/AMECDataTypes/DataType/SubItems/SubItem/ByteOffset`

  The offset of `SubItem`, in byte, relative to the start of its parent `DataType` internal address space. The element text must be 32-bit unsigned decimal integral number literal.



- `/AMECDataTypes/DataType/SubItems/SubItem/Comment`

  Comment of `SubItem`, element text could be arbitrary string.



### Variable Catalogue

**File name**

*variable_catalogue.xml*



**Syntax**

```xml
<?xml version="1.0" encoding="UTF-8"?>
<AMECVariables FormatVersion="1">
    <Variable>
        <ID>0</ID>
        <Name>Lid Closed</Name>
        <DataType>BIT</DataType>
        <Unit>N/A</Unit>
        <Comment>Lid closed signal</Comment>
    </Variable>
    <Variable>
        <ID>2020</ID>
        <Name>ST1 Soft Pin Pressure</Name>
        <DataType>FLOAT</DataType>
        <Unit>MPa</Unit>
        <Comment>Station1 softpin pressure meter reading</Comment>
    </Variable>
    <Variable>
        <ID>3000</ID>
        <Name>ST1 ESC HV Setpoint</Name>
        <DataType>FLOAT</DataType>
        <Unit>Voltage</Unit>
        <Comment>Station1 E-Chuck HVM setpoint</Comment>
    </Variable>
</AMECVariables>
```



**Elements**

- `/AMECVariables`

  The name of root element must be `"AMECVariables"`, The element is parent element of `Variable`. It could contain any number of child `Variable` elements.

  

- `/AMECVariables/attribute::FormatVersion`

  The mandatory attribute is used to mark the file format version. The value must be 32-bit unsigned decimal integral number literal. 



- `/AMECVariables/Variable`

  Child element of `AMECVariables`, the element contains the variable Information. The number of `Variable`  is unlimited.



- `/AMECVariables/Variable/ID`

  Variable ID. All variables must have different `ID`. The element text must be 32-bit unsigned decimal integral number literal.



- `/AMECVariables/Variable/Name`

  Variable name. All variables must have different `Name`. The variable `Name` text can be arbitrary string. Others can reference this variable using this string.



- `/AMECVariables/Variable/DataType`

  Variable data type. The element text literal is the name of data type. The data type with same name must be a valid `DataType` defined in **Data Type Catalogue (data_type_catalogue.xml)**.



- `/AMECVariables/Variable/Unit`

  Variable unit. The element text can be arbitrary string. 



- `/AMECVariables/Variable/Comment`

  Comment of variable. The element text can be arbitrary string. 



## IO List

**Syntax**

```xml
<?xml version="1.0" encoding="UTF-8"?>
<AMECIOList FormatVersion="1">
    <TargetInfo></TargetInfo>
    <ControllerInfo></ControllerInfo>
    <Objects></Objects>
    <TxPDO>
        <DiagArea WordOffset="1000" WordSize="32"></DiagArea>
        <BitArea WordOffset="0" WordSize="32"></BitArea>
        <BlockArea WordOffset="32" WordSize="256"></BlockArea>
    </TxPDO>
    <RxPDO>
        <ControlArea WordOffset="2000" WordSize="32"></ControlArea>
        <BitArea WordOffset="288" WordSize="32"></BitArea>
        <BlockArea WordOffset="320" WordSize="256"></BlockArea>
    </RxPDO>
    <Interlocks></Interlocks>
</AMECIOList>
```



**Elements**

- `/AMECIOList`

  The name of root element must be `"AMECIOList"`.



- `/AMECIOList/attribute::FormatVersion`

  The mandatory attribute is used to mark the file format version. The value must be 32-bit unsigned decimal integral number literal. 



- `/AMECIOList/TargetInfo`

  Holding file custom information, see [TargetInfo](#targetinfo) for child elements.



- `/AMECIOList/ControllerInfo`

  Holding controller modules information, see [ControllerInfo](#controllerinfo) for child elements.



- `/AMECIOList/Objects`

  Object dictionary, see [Objects](#objects) for child elements.

  

- `/AMECIOList/TxPDO` `/AMECIOList/RxPDO`

  PDO collection, see [RxPDO/TxPDO](#rxpdotxpdo) for child elements.



- `/AMECIOList/Interlocks`

  Interlock collection, see [Interlocks](#interlocks) for child elements.



### TargetInfo

**Syntax**

```xml
<TargetInfo>
    <Name>SD-RIE</Name>
    <Description>XXXX</Description>
</TargetInfo>
```



**Elements**

- `TargetInfo/Name`  `TargetInfo/Description` 

  The element text can be arbitrary string and depends on user. 



### **ControllerInfo**

**Syntax**

```xml
<ControllerInfo>
    <MCServer>
        <IP>192.168.2.112</IP>
        <Port>5010</Port>
    </MCServer>
    <ExtensionModules>
        <ExtensionModule>
            <ID>0x1001</ID>
            <Swtich>0x000000000</Swtich>
            <Name>R12CCPU_V_SMART_ECAT_0</Name>
            <Address>0x3E10</Address>
        </ExtensionModule>
        <ExtensionModule>
            <ID>0x0021</ID>
            <Name>R60ADV8_0</Name>
            <Address>0x0000</Address>
        </ExtensionModule>
    </ExtensionModules>
    <EthernetModules>
        <EthernetModule>
            <ID>0x2001</ID>
            <Swtich>0x000000000</Swtich>
            <Name>PC104_SERIAL_PORT_SERVER</Name>
            <IP>192.168.3.200</IP>
            <Port>502</Port>
        </EthernetModule>
    </EthernetModules>
</ControllerInfo>
```



**Elements**

- `ControllerInfo/MCServer/IP`

  Controller IPv4 address. The element text must be a valid IPv4 address literal.

  

- `ControllerInfo/MCServer/Port`

  MC protocol TCP port. The element text must be a 16-bit unsigned decimal integral number literal. 

  

- `ControllerInfo/ExtensionModules`

  The parent element of `ExtensionModule`. It could contain any number of child `ExtensionModule` elements.

  

- `ControllerInfo/EthernetModules`

  The parent element of `EthernetModules`. It could contain any number of child `EthernetModules` elements.



- `ControllerInfo/ExtensionModules/ExtensionModule`

  Child element of `ExtensionModules`, the element contains the Extension Module Information. The number of `ExtensionModule`  is unlimited.

  

- `ControllerInfo/ExtensionModules/ExtensionModule/ID`

  The element text must be 16-bit unsigned hexadecimal integral number literal (begins with 0x).  

  The `ExtensionModel` with same `ID` must be defined in **Controller Model Catalogue (controller_model_catalogue.xml)** in advance.

  

- `ControllerInfo/ExtensionModules/ExtensionModule/Switch`

  `ExtensionModule` switch setting. The element text must be 32-bit unsigned hexadecimal integral number literal (begins with 0x).  

  

- `ControllerInfo/ExtensionModules/ExtensionModule/Name`

  The element text can be arbitrary string.  

  This string is an alias for the module. Object Binding (see [Objects](#objects)) can use the alias to reference this `ExtensionModule`.

  

- `ControllerInfo/ExtensionModules/ExtensionModule/Address`

  `ExtensionModule` local address. The element text must be 16-bit unsigned hexadecimal integral number literal (begins with 0x).  

  

- `ControllerInfo/EthernetModules/EthernetModule`

  Child element of `EthernetModules`, the element contains the Ethernet Module Information. The number of `EthernetModule`  is unlimited.



- `ControllerInfo/EthernetModules/EthernetModule/ID`

  The element text must be 16-bit unsigned hexadecimal integral number literal (begins with 0x).  

  The `EthernetModel` with same `ID` must be defined in **Controller Model Catalogue (controller_model_catalogue.xml)** in advance.
  
  

- `ControllerInfo/EthernetModules/EthernetModule/Switch`

  `EthernetModule` switch setting. The element text must be 32-bit unsigned hexadecimal integral number literal (begins with 0x).  

  

- `ControllerInfo/EthernetModules/EthernetModule/Name`

  The element text can be arbitrary string.  

  This string is an alias for the module. Object Binding (see [Objects](#objects)) can use the alias to reference this `EthernetModule`.



- `ControllerInfo/EthernetModules/EthernetModule/IP`

  `EthernetModule` IPv4 address. The element text must be a valid IPv4 address literal.



- `ControllerInfo/EthernetModules/EthernetModule/Port`

  `EthernetModule` protocol TCP. The element text must be a 16-bit unsigned decimal integral number literal. 



### Objects

**Syntax**

```xml
<Objects>
    <Object>
        <Index>0x80000000</Index>
        <Name>Lid Closed</Name>
        <Binding>
            <Module>RX42C4_0</Module>
            <Bit>0</Bit>
        </Binding>
    </Object>
    <Object>
        <Index>0x80002807</Index>
        <Name>MFC6 Flow Reading</Name>
        <Binding>
            <Module>RJ71DN91_MASTER_0</Module>
            <Raw>37</Raw>
        </Binding>
        <Range>
            <UpLimit>1000</UpLimit>
            <DownLimit>0</DownLimit>
        </Range>
    </Object>
    <Object>
        <Index>0x80002016</Index>
        <Name>Onboard Gas1 Supply Pressure</Name>
        <Binding>
            <Module>R60ADV8_2</Module>
            <Channel>6</Channel>
        </Binding>
        <Range>
            <UpLimit>100</UpLimit>
            <DownLimit>0</DownLimit>
        </Range>
        <Converter>
            <UpScale>100</UpScale>
            <DownScale>-100</DownScale>
        </Converter>
    </Object>
</Objects>
```



**Elements**

- `Objects/Object`

  The element and its child elements define `Object`, `Object` is the most basic element of [RxPDO/TxPDO](#rxpdotxpdo) and [Interlocks](#interlocks).

  

- `Objects/Object/Index`

  The element text must be 32-bit unsigned hexadecimal integral number literal (begins with 0x).  All objects must have different `Index`.

  

- `Objects/Object/Name`

  The element text string is a variable name, the `Variable` with same `Name` should be defined in **Variable Catalogue (variable_catalogue.xml)**. In another words, if the element text string is a valid variable name in **Variable Catalogue (variable_catalogue.xml)**, the data type and other details of object variable can be determined.

  

- `Objects/Object/Binding`

  This element is optional, it describes the binding information. 

  Every `Object` can optionally be bound to a controller module data channel. Multiple Objects can be bound to same controller module data channel.

  The binding controller module is defined as a child element:

  ```xml
  <Module>module_alias</Module>
  ```

  "module_alias" is the module alias, a module with same alias must be defined in [ControllerInfo](#controllerinfo) in advance.

  The data channel name and data channel index are also defined as a child element:

  ```xml
  <channel_name>channel_index</channel_name>
  ```

  The element name is the channel name and its text is channel index.

  Available channel names and the range of channel index depend on the controller model of binding controller module (see [Controller Model Catalogue](#controller-model-catalogue)). 

  If the channel name is a Tx data channel name, the object then can only be added to TxPDO, Likewise, if the channel name is a Rx data channel name, the object then can only be added to RxPDO.

  If this element is omitted, the `Object` can be added to either TxPDO or RxPDO.

  

- `Objects/Object/Converter`

  This element is optional, it describes the scale range. It may help control runtime to convert controller module raw data value to object variable logical value or vice versa.

  It have two child elements, one is :

  ```xml
  <UpScale>100</UpScale>
  ```

  and the other is:

  ```xml
  <DownScale>-100</DownScale>
  ```

  Text of both child elements are decimal double float number literal.

  

- `Objects/Object/Range`

  This element is optional, it describes the range of object variable logical value.

  It have two child elements, one is :

  ```xml
  <UpLimit>100</UpLimit>
  ```

  and the other is:

  ```xml
  <DownLimit>0</DownLimit>
  ```

  Text of both child elements can be arbitrary string.  



### RxPDO/TxPDO

**Syntax**

```xml
<TxPDO>
    <DiagArea WordOffset="1000" WordSize="32">
        <Index>0xC0000010</Index>
        <Index>0xC0000011</Index>
    </DiagArea>
    <BitArea WordOffset="0" WordSize="32">
        <Index>0x80000000</Index>
        <Index>0x80000001</Index>
    </BitArea>
    <BlockArea WordOffset="32" WordSize="256">
        <Index>0x80002000</Index>
        <Index>0x80002001</Index>
    </BlockArea>
</TxPDO>
<RxPDO>
    <ControlArea WordOffset="2000" WordSize="32">
        <Index>0x40000022</Index>
        <Index>0x40000021</Index>
        <Index>0x40000020</Index>
    </ControlArea>
    <BitArea WordOffset="288" WordSize="32">
        <Index>0x00001000</Index>
        <Index>0x00001001</Index>
    </BitArea>
    <BlockArea WordOffset="320" WordSize="256">
        <Index>0x00003000</Index>
        <Index>0x00003001</Index>
    </BlockArea>
</RxPDO>
```

The IO list provides six PDO areas for users.

All child elements of these areas are [Object](#Objects) references.

The name of these elements must be `Index` and the text of these elements must be 32-bit unsigned hexadecimal integral number literal (begins with 0x), as follows:

```xml
<Index>0x00003000</Index>
<Index>0x00003001</Index>
```

The object with same `Index` element should be defined in [Object dictionary](Objects) in advance. All indexes defined in PDO must have different values. All objects referred in the corresponding PDO area should have different index values;

Objects layout in PDO areas is in sequence as defined.

The alignments of objects in `RxPDO/ControlArea`, `RxPDO/BlockArea`, `TxPDO/DiaglArea` and `TxPDO/BlockArea` are always as same as objects data type size and the byte order of these objects is always little endian. The objects of data type size == 1 can not be added to these areas. 

The objects of data type size == 1 can only be added to `RxPDO/BitArea` or `TxPDO/BitArea`, The alignment of object in `RxPDO/BitArea` and `TxPDO/BitArea` is always 1bit.



**Elements**

- `TxPDO/DiagArea/attribute::WordOffset`

- `TxPDO/BitArea/attribute::WordOffset`

- `TxPDO/BlockArea/attribute::WordOffset`

- `RxPDO/ControlArea/attribute::WordOffset`

- `RxPDO/BlockArea/attribute::WordOffset`

- `RxPDO/BitArea/attribute::WordOffset`

  Area memory start address in word(2bytes).  The attribute value must be 32-bit unsigned decimal integral number literal. 

  

- `TxPDO/DiagArea/attribute::WordSize`

- `TxPDO/BitArea/attribute::WordSize`

- `TxPDO/BlockArea/attribute::WordSize`

- `RxPDO/ControlArea/attribute::WordSize`

- `RxPDO/BlockArea/attribute::WordSize`

- `RxPDO/BitArea/attribute::WordSize`

  Area memory size in word(2bytes).  The attribute value must be 32-bit unsigned decimal integral number literal. 

  

### Interlocks

**Syntax**

```xml
<Interlocks>
    <Interlock>
        <Name>Tub Pump Foreline Fast Act Vlv</Name>
        <Target>
            <Index>0x00001005</Index>
        </Target>
        <Statement>
            <AND>
                <Index>0x80000018</Index>
            </AND>
        </Statement>
    </Interlock>
    <Interlock>
        <Name>D-ESC 1 Heater</Name>
        <Target>
            <Index>0x0000100C</Index>
            <Index>0x0000100D</Index>
        </Target>
        <Statement>
            <AND>
                <Index>0x80000000</Index>
                <Index>0x80000004</Index>
                <Index>0x80000014</Index>
                <Index>0x80000026</Index>
            </AND>
        </Statement>
    </Interlock>
</Interlocks>
```



**Elements**

- `Interlocks`

  The parent element of `Interlock`. It could contain any number of child `Interlock` elements.

  

- `Interlocks/Interlock`

  The element and its child elements define a `Interlock` logic.

  

- `Interlocks/Interlock/Name`

  The element defines interlock logic name, the element text can be arbitrary string. 

  

- `Interlocks/Interlock/Target`

  All child elements of the element are [Object](#Objects) references. 

  If the interlock expression described by `Interlocks/Interlock/Statement` is evaluated to a value of *false*, objects referenced here should be set to 0 by controller runtime.

  

- `Interlocks/Interlock/Statement`

  Child elements of the element describe a logical expression by a tree like hierarchy structure. The root of tree must be a logical operator element
  
  The available logical operators:
  
  ```xml
  <AND></AND>
  <OR></OR>
  <NOT></NOT>
  <XOR></XOR>
  <NAND></NAND>
  <NOR></NOR>
  ```
  
  Here is an example :
  
  ```xml
  <OR>
      <Index>0x80000000</Index>
      <Index>0x80000004</Index>
  </OR>
  ```
  
  ```c
  0x80000000 || 0x80000004 
  ```
  
  Nested logical expression is allowed : 
  
  ```xml
  <OR>
      <Index>0x80000000</Index>
      <Index>0x80000004</Index>
      <AND>
          <Index>0x80000014</Index>
          <Index>0x80000016</Index>
      </AND>
  </OR>
  ```
  
  ```c
  0x80000000 || 0x80000004 || (0x80000014 && 0x80000016)
  ```



- `Interlocks/Interlock//Index`

  The element is a [Object](#Objects) reference. 

  Element text must be 32-bit unsigned hexadecimal integral number literal (begins with 0x). The object with same `index` element should be defined in [Object dictionary](Objects) in advance, in addition, The data type size of referenced object must be 1bit.



## Revision

TBD