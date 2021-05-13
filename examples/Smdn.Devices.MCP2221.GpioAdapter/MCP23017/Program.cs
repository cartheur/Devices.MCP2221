﻿// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Threading.Tasks;

using Smdn.Devices.MCP2221;
using Smdn.Devices.MCP2221.GpioAdapter;

using Iot.Device.Mcp23xxx;

await using var device = await MCP2221.OpenAsync();

await device.GP3.ConfigureAsLEDI2CAsync();

const int deviceAddressMcp23017 = 0x20; // The address of MCP23017 which is connected to MCP2221/MCP2221A

var i2cDevice = new MCP2221I2cDevice(device.I2C, deviceAddressMcp23017);

i2cDevice.BusSpeed = I2CBusSpeed.Default;

var mcp23017 = new Mcp23017(
  i2cDevice: i2cDevice,
  shouldDispose: false, // Mcp23017 itself does not dispose supplied i2cDevice above in this case
  controller: null,
  reset: -1,        // disable RESET pin
  interruptA: -1,   // disable INTA pin
  interruptB: -1    // disable INTB pin
);

// disable interrupt-on-change of GPINT<0~7>
mcp23017.WriteUInt16(Register.GPINTEN, 0b_0000_0000_0000_0000);

// configure GPA<0~7> and GPB<0~7> as output (IODIRA, IODIRB)
mcp23017.WriteUInt16(Register.IODIR, 0b_0000_0000_0000_0000);

// set GPA<0~7> = 0b_1111_1111(all HIGH), set GPB<0~7> = 0b_0000_0000(all LOW)
mcp23017.WriteUInt16(Register.GPIO, 0b_1111_1111_0000_0000);

await Task.Delay(1000);

// set GPA<0~7> = 0b_0000_0000(all LOW), set GPB<0~7> = 0b_1111_1111(all HIGH)
mcp23017.WriteUInt16(Register.GPIO, 0b_0000_0000_1111_1111);

await Task.Delay(1000);

for (;;) {
  // set each GPIO HIGH one by one
  for (var bit = 0; bit < 16; bit++) {
    mcp23017.WriteUInt16(Register.GPIO, (ushort)(0b1 << bit));

    await Task.Delay(100);
  }
}
