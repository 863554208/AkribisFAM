using AkribisFAM.Manager;
using AkribisFAM.Util;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Reflection;
using System.Threading.Tasks;
using static AkribisFAM.CommunicationProtocol.IOManager;

namespace AkribisFAM.CommunicationProtocol
{
    public enum IO_OutFunction_Table
    {
        [Description(""), EEGroup("OUT1"), EENumber("OUT1_0"), Tags("Conveyor", "Laser Station")] OUT1_0Left_1_lift_cylinder_extend = 0,
        [Description(""), EEGroup("OUT1"), EENumber("OUT1_1"), Tags("Conveyor", "Laser Station")] OUT1_1Left_1_lift_cylinder_retract,
        [Description(""), EEGroup("OUT1"), EENumber("OUT1_2"), Tags("Conveyor", "Laser Station")] OUT1_2Right_1_lift_cylinder_extend,
        [Description(""), EEGroup("OUT1"), EENumber("OUT1_3"), Tags("Conveyor", "Laser Station")] OUT1_3Right_1_lift_cylinder_retract,
        [Description(""), EEGroup("OUT1"), EENumber("OUT1_4"), Tags("Conveyor", "Foam Assembly Station")] OUT1_4Left_2_lift_cylinder_extend,
        [Description(""), EEGroup("OUT1"), EENumber("OUT1_5"), Tags("Conveyor", "Foam Assembly Station")] OUT1_5Left_2_lift_cylinder_retract,
        [Description(""), EEGroup("OUT1"), EENumber("OUT1_6"), Tags("Conveyor", "Foam Assembly Station")] OUT1_6Right_2_lift_cylinder_extend,
        [Description(""), EEGroup("OUT1"), EENumber("OUT1_7"), Tags("Conveyor", "Foam Assembly Station")] OUT1_7Right_2_lift_cylinder_retract,
        [Description(""), EEGroup("OUT1"), EENumber("OUT1_8"), Tags("Conveyor", "Recheck Station")] OUT1_8Left_3_lift_cylinder_extend,
        [Description(""), EEGroup("OUT1"), EENumber("OUT1_9"), Tags("Conveyor", "Recheck Station")] OUT1_9Left_3_lift_cylinder_retract,
        [Description(""), EEGroup("OUT1"), EENumber("OUT1_10"), Tags("Conveyor", "Recheck Station")] OUT1_10Right_3_lift_cylinder_extend,
        [Description(""), EEGroup("OUT1"), EENumber("OUT1_11"), Tags("Conveyor", "Recheck Station")] OUT1_11Right_3_lift_cylinder_retract,
        [Description(""), EEGroup("OUT1"), EENumber("OUT1_12"), Tags("Conveyor", "NG Station")] OUT1_124_lift_cylinder_extend,
        [Description(""), EEGroup("OUT1"), EENumber("OUT1_13"), Tags("Conveyor", "NG Station")] OUT1_134_lift_cylinder_retract,
        [Description(""), EEGroup("OUT1"), EENumber("OUT1_14"), Tags("")] OUT1_14Reserve,
        [Description(""), EEGroup("OUT1"), EENumber("OUT1_15"), Tags("")] OUT1_15Reserve,

        [Description(""), EEGroup("OUT2"), EENumber("OUT2_0"), Tags("Conveyor", "Laser Station")] OUT2_0Stopping_Cylinder1_extend,
        [Description(""), EEGroup("OUT2"), EENumber("OUT2_1"), Tags("Conveyor", "Laser Station")] OUT2_1Stopping_Cylinder1_retract,
        [Description(""), EEGroup("OUT2"), EENumber("OUT2_2"), Tags("Conveyor", "Foam Assembly Station")] OUT2_2Stopping_Cylinder2_extend,
        [Description(""), EEGroup("OUT2"), EENumber("OUT2_3"), Tags("Conveyor", "Foam Assembly Station")] OUT2_3Stopping_Cylinder2_retract,
        [Description(""), EEGroup("OUT2"), EENumber("OUT2_4"), Tags("Conveyor", "Recheck Station")] OUT2_4Stopping_Cylinder3_extend,
        [Description(""), EEGroup("OUT2"), EENumber("OUT2_5"), Tags("Conveyor", "Recheck Station")] OUT2_5Stopping_Cylinder3_retract,
        [Description(""), EEGroup("OUT2"), EENumber("OUT2_6"), Tags("Conveyor", "NG Station")] OUT2_6Stopping_Cylinder4_extend,
        [Description(""), EEGroup("OUT2"), EENumber("OUT2_7"), Tags("Conveyor", "NG Station")] OUT2_7Stopping_Cylinder4_retract,
        [Description(""), EEGroup("OUT2"), EENumber("OUT2_8"), Tags("Door")] OUT2_8LOCK1,
        [Description(""), EEGroup("OUT2"), EENumber("OUT2_9"), Tags("Door")] OUT2_9LOCK2,
        [Description(""), EEGroup("OUT2"), EENumber("OUT2_10"), Tags("Door")] OUT2_10LOCK3,
        [Description(""), EEGroup("OUT2"), EENumber("OUT2_11"), Tags("Door")] OUT2_11LOCK4,
        [Description(""), EEGroup("OUT2"), EENumber("OUT2_12"), Tags("")] OUT2_12Reserve,
        [Description(""), EEGroup("OUT2"), EENumber("OUT2_13"), Tags("")] OUT2_13Reserve,
        [Description(""), EEGroup("OUT2"), EENumber("OUT2_14"), Tags("")] OUT2_14Reserve,
        [Description(""), EEGroup("OUT2"), EENumber("OUT2_15"), Tags("System")] OUT2_15FFU, //Fan

        [Description(""), EEGroup("OUT3"), EENumber("OUT3_0"), Tags("Picker", "Vacuum Air", "Foam Assembly Station")] OUT3_0PNP_Gantry_vacuum1_Supply,
        [Description(""), EEGroup("OUT3"), EENumber("OUT3_1"), Tags("Picker", "Vacuum Air", "Foam Assembly Station")] OUT3_1PNP_Gantry_vacuum1_Release,
        [Description(""), EEGroup("OUT3"), EENumber("OUT3_2"), Tags("Picker", "Vacuum Air", "Foam Assembly Station")] OUT3_2PNP_Gantry_vacuum2_Supply,
        [Description(""), EEGroup("OUT3"), EENumber("OUT3_3"), Tags("Picker", "Vacuum Air", "Foam Assembly Station")] OUT3_3PNP_Gantry_vacuum2_Release,
        [Description(""), EEGroup("OUT3"), EENumber("OUT3_4"), Tags("Picker", "Vacuum Air", "Foam Assembly Station")] OUT3_4PNP_Gantry_vacuum3_Supply,
        [Description(""), EEGroup("OUT3"), EENumber("OUT3_5"), Tags("Picker", "Vacuum Air", "Foam Assembly Station")] OUT3_5PNP_Gantry_vacuum3_Release,
        [Description(""), EEGroup("OUT3"), EENumber("OUT3_6"), Tags("Picker", "Vacuum Air", "Foam Assembly Station")] OUT3_6PNP_Gantry_vacuum4_Supply,
        [Description(""), EEGroup("OUT3"), EENumber("OUT3_7"), Tags("Picker", "Vacuum Air", "Foam Assembly Station")] OUT3_7PNP_Gantry_vacuum4_Release,
        [Description(""), EEGroup("OUT3"), EENumber("OUT3_8"), Tags("")] OUT3_8Reserve, //?
        [Description(""), EEGroup("OUT3"), EENumber("OUT3_9"), Tags("")] OUT3_9Reserve,
        [Description(""), EEGroup("OUT3"), EENumber("OUT3_10"), Tags("")] OUT3_10Reserve,
        [Description(""), EEGroup("OUT3"), EENumber("OUT3_11"), Tags("")] OUT3_11Reserve,
        [Description(""), EEGroup("OUT3"), EENumber("OUT3_12"), Tags("")] OUT3_12Reserve,
        [Description(""), EEGroup("OUT3"), EENumber("OUT3_13"), Tags("")] OUT3_13Reserve,
        [Description(""), EEGroup("OUT3"), EENumber("OUT3_14"), Tags("")] OUT3_14Reserve,
        [Description(""), EEGroup("OUT3"), EENumber("OUT3_15"), Tags("")] OUT3_15Reserve,

        [Description(""), EEGroup("OUT4"), EENumber("OUT4_0"), Tags("Recheck Station")] OUT4_0Pneumatic_Claw_A,
        [Description(""), EEGroup("OUT4"), EENumber("OUT4_1"), Tags("Recheck Station")] OUT4_1Pneumatic_Claw_B,
        [Description(""), EEGroup("OUT4"), EENumber("OUT4_2"), Tags("Vacuum Air", "Recheck Station")] OUT4_2Peeling_Recheck_vacuum1_Supply,
        [Description(""), EEGroup("OUT4"), EENumber("OUT4_3"), Tags("System")] OUT4_3Machine_Reset,
        [Description(""), EEGroup("OUT4"), EENumber("OUT4_4"), Tags("")] OUT4_4Reserve,
        [Description(""), EEGroup("OUT4"), EENumber("OUT4_5"), Tags("")] OUT4_5Reserve,
        [Description(""), EEGroup("OUT4"), EENumber("OUT4_6"), Tags("")] OUT4_6Reserve,
        [Description(""), EEGroup("OUT4"), EENumber("OUT4_7"), Tags("")] OUT4_7Reserve,
        [Description(""), EEGroup("OUT4"), EENumber("OUT4_8"), Tags("Feeder 1", "Foam Assembly Station")] OUT4_8Stop_feeder1,
        [Description(""), EEGroup("OUT4"), EENumber("OUT4_9"), Tags("Feeder 1", "Foam Assembly Station")] OUT4_9Run_feeder1,
        [Description(""), EEGroup("OUT4"), EENumber("OUT4_10"), Tags("Feeder 1", "Foam Assembly Station")] OUT4_10initialize_feeder1,
        [Description(""), EEGroup("OUT4"), EENumber("OUT4_11"), Tags("")] OUT4_11Backup,
        [Description(""), EEGroup("OUT4"), EENumber("OUT4_12"), Tags("Feeder 2", "Foam Assembly Station")] OUT4_12Stop_feeder2,
        [Description(""), EEGroup("OUT4"), EENumber("OUT4_13"), Tags("Feeder 2", "Foam Assembly Station")] OUT4_13Run_feeder2,
        [Description(""), EEGroup("OUT4"), EENumber("OUT4_14"), Tags("Feeder 2", "Foam Assembly Station")] OUT4_14initialize_feeder2,
        [Description(""), EEGroup("OUT4"), EENumber("OUT4_15"), Tags("")] OUT4_15Backup,

        [Description(""), EEGroup("OUT5"), EENumber("OUT5_0"), Tags("Feeder 1", "Foam Assembly Station")] OUT5_0Feeder1_limit_cylinder_extend,
        [Description(""), EEGroup("OUT5"), EENumber("OUT5_1"), Tags("Feeder 1", "Foam Assembly Station")] OUT5_1Feeder1_limit_cylinder_retract,
        [Description(""), EEGroup("OUT5"), EENumber("OUT5_2"), Tags("Feeder 1", "Foam Assembly Station")] OUT5_2Feeder2_limit_cylinder_extend,
        [Description(""), EEGroup("OUT5"), EENumber("OUT5_3"), Tags("Feeder 1", "Foam Assembly Station")] OUT5_3Feeder2_limit_cylinder_retract,
        [Description(""), EEGroup("OUT5"), EENumber("OUT5_4"), Tags("")] OUT5_4Backup,
        [Description(""), EEGroup("OUT5"), EENumber("OUT5_5"), Tags("Vision", "Foam Assembly Station")] OUT5_5PnP_Gantry_Camera_Trig,
        [Description(""), EEGroup("OUT5"), EENumber("OUT5_6"), Tags("Vision", "Foam Assembly Station")] OUT5_6Feeder_Camera_Trig,
        [Description(""), EEGroup("OUT5"), EENumber("OUT5_7"), Tags("Vision", "Recheck Station")] OUT5_7Recheck_Camera_Trig,//rename to camera trigger
        [Description(""), EEGroup("OUT5"), EENumber("OUT5_8"), Tags("Feeder 1", "Foam Assembly Station")] OUT5_8Feeder_vacuum1_Supply,
        [Description(""), EEGroup("OUT5"), EENumber("OUT5_9"), Tags("Feeder 1", "Foam Assembly Station")] OUT5_9Feeder_vacuum1_Release,
        [Description(""), EEGroup("OUT5"), EENumber("OUT5_10"), Tags("Feeder 1", "Foam Assembly Station")] OUT5_10Feeder_vacuum2_Supply,
        [Description(""), EEGroup("OUT5"), EENumber("OUT5_11"), Tags("Feeder 1", "Foam Assembly Station")] OUT5_11Feeder_vacuum2_Release,
        [Description(""), EEGroup("OUT5"), EENumber("OUT5_12"), Tags("Feeder 2", "Foam Assembly Station")] OUT5_12Feeder_vacuum3_Supply,
        [Description(""), EEGroup("OUT5"), EENumber("OUT5_13"), Tags("Feeder 2", "Foam Assembly Station")] OUT5_13Feeder_vacuum3_Release,
        [Description(""), EEGroup("OUT5"), EENumber("OUT5_14"), Tags("Feeder 2", "Foam Assembly Station")] OUT5_14Feeder_vacuum4_Supply,
        [Description(""), EEGroup("OUT5"), EENumber("OUT5_15"), Tags("Feeder 2", "Foam Assembly Station")] OUT5_15Feeder_vacuum4_Release,


        [Description(""), EEGroup("OUT6"), EENumber("OUT6_0"), Tags("Light")] OUT6_0Tri_color_light_red, // machine tower light bar
        [Description(""), EEGroup("OUT6"), EENumber("OUT6_1"), Tags("Light")] OUT6_1Tri_color_light_yellow,
        [Description(""), EEGroup("OUT6"), EENumber("OUT6_2"), Tags("Light")] OUT6_2Tri_color_light_green,
        [Description(""), EEGroup("OUT6"), EENumber("OUT6_3"), Tags("Light")] OUT6_3light1, //machine front
        [Description(""), EEGroup("OUT6"), EENumber("OUT6_4"), Tags("Light")] OUT6_4light2, //machine back
        [Description(""), EEGroup("OUT6"), EENumber("OUT6_5"), Tags("Buzzer")] OUT6_5Buzzer,
        [Description(""), EEGroup("OUT6"), EENumber("OUT6_6"), Tags("")] OUT6_6Reserve,
        [Description(""), EEGroup("OUT6"), EENumber("OUT6_7"), Tags("")] OUT6_7Reserve,
        [Description(""), EEGroup("OUT6"), EENumber("OUT6_8"), Tags("Light", "Button")] OUT6_8Run_light,
        [Description(""), EEGroup("OUT6"), EENumber("OUT6_9"), Tags("Light", "Button")] OUT6_9Stop_light,
        [Description(""), EEGroup("OUT6"), EENumber("OUT6_10"), Tags("Light", "Button")] OUT6_10Feeder1_light,
        [Description(""), EEGroup("OUT6"), EENumber("OUT6_11"), Tags("Light", "Button")] OUT6_11Feeder2_light,
        [Description(""), EEGroup("OUT6"), EENumber("OUT6_12"), Tags("Light", "Button")] OUT6_12Reset_light,
        [Description(""), EEGroup("OUT6"), EENumber("OUT6_13"), Tags("")] OUT6_13Reserve,
        [Description(""), EEGroup("OUT6"), EENumber("OUT6_14"), Tags("")] OUT6_14Reserve,
        [Description(""), EEGroup("OUT6"), EENumber("OUT6_15"), Tags("")] OUT6_15Reserve,

        [Description(""), EEGroup("OUT7"), EENumber("OUT7_0"), Tags("Conveyor", "SMEMA")] OUT7_0MACHINE_READY_TO_RECEIVE,
        [Description(""), EEGroup("OUT7"), EENumber("OUT7_1"), Tags("Conveyor", "SMEMA")] OUT7_1BOARD_AVAILABLE,
        [Description(""), EEGroup("OUT7"), EENumber("OUT7_2"), Tags("Conveyor", "SMEMA")] OUT7_2FAILED_BOARD_AVAILABLE_OPTIONAL,
        [Description(""), EEGroup("OUT7"), EENumber("OUT7_3"), Tags("")] OUT7_3Reserve,
        [Description(""), EEGroup("OUT7"), EENumber("OUT7_4"), Tags("")] OUT7_4Reserve,
        [Description(""), EEGroup("OUT7"), EENumber("OUT7_5"), Tags("")] OUT7_5Reserve,
        [Description(""), EEGroup("OUT7"), EENumber("OUT7_6"), Tags("")] OUT7_6Reserve,
        [Description(""), EEGroup("OUT7"), EENumber("OUT7_7"), Tags("")] OUT7_7Reserve,
        [Description(""), EEGroup("OUT7"), EENumber("OUT7_8"), Tags("")] OUT7_8Reserve,
        [Description(""), EEGroup("OUT7"), EENumber("OUT7_9"), Tags("")] OUT7_9Reserve,
        [Description(""), EEGroup("OUT7"), EENumber("OUT7_10"), Tags("")] OUT7_10Reserve,
        [Description(""), EEGroup("OUT7"), EENumber("OUT7_11"), Tags("")] OUT7_11Reserve,
        [Description(""), EEGroup("OUT7"), EENumber("OUT7_12"), Tags("")] OUT7_12Reserve,
        [Description(""), EEGroup("OUT7"), EENumber("OUT7_13"), Tags("")] OUT7_13Reserve,
        [Description(""), EEGroup("OUT7"), EENumber("OUT7_14"), Tags("")] OUT7_14Reserve,
        [Description(""), EEGroup("OUT7"), EENumber("OUT7_15"), Tags("")] OUT7_15Reserve

    }
    public enum IO_INFunction_Table
    {
        [Description(""), EEGroup("IN1"), EENumber("IN1_0"), Tags("Conveyor", "Laser Station")] IN1_0Slowdown_Sign1 = 0,
        [Description(""), EEGroup("IN1"), EENumber("IN1_1"), Tags("Conveyor", "Foam Assembly Station")] IN1_1Slowdown_Sign2,
        [Description(""), EEGroup("IN1"), EENumber("IN1_2"), Tags("Conveyor", "Recheck Station")] IN1_2Slowdown_Sign3,
        [Description(""), EEGroup("IN1"), EENumber("IN1_3"), Tags("Conveyor", "NG Station")] IN1_3Slowdown_Sign4,
        [Description(""), EEGroup("IN1"), EENumber("IN1_4"), Tags("Conveyor", "Laser Station")] IN1_4Stop_Sign1,
        [Description(""), EEGroup("IN1"), EENumber("IN1_5"), Tags("Conveyor", "Foam Assembly Station")] IN1_5Stop_Sign2,
        [Description(""), EEGroup("IN1"), EENumber("IN1_6"), Tags("Conveyor", "Recheck Station")] IN1_6Stop_Sign3,
        [Description(""), EEGroup("IN1"), EENumber("IN1_7"), Tags("Conveyor", "NG Station")] IN1_7Stop_Sign4,
        [Description(""), EEGroup("IN1"), EENumber("IN1_8"), Tags("Conveyor", "NG Station")] IN1_8NG_cover_plate1,
        [Description(""), EEGroup("IN1"), EENumber("IN1_9"), Tags("Conveyor", "NG Station")] IN1_9NG_cover_plate2,
        [Description(""), EEGroup("IN1"), EENumber("IN1_10"), Tags("Conveyor", "Laser Station")] IN1_10plate_has_left_Behind_the_stopping_cylinder1,
        [Description(""), EEGroup("IN1"), EENumber("IN1_11"), Tags("Conveyor", "Foam Assembly Station")] IN1_11plate_has_left_Behind_the_stopping_cylinder2,
        [Description(""), EEGroup("IN1"), EENumber("IN1_12"), Tags("Conveyor", "Laser Station")] IN1_12bord_lift_in_position1,
        [Description(""), EEGroup("IN1"), EENumber("IN1_13"), Tags("Conveyor", "Foam Assembly Station")] IN1_13bord_lift_in_position2,
        [Description(""), EEGroup("IN1"), EENumber("IN1_14"), Tags("Conveyor", "Recheck Station")] IN1_14bord_lift_in_position3,
        [Description(""), EEGroup("IN1"), EENumber("IN1_15"), Tags("Conveyor", "NG Station")] IN1_15bord_lift_in_position4,

        [Description(""), EEGroup("IN2"), EENumber("IN2_0"), Tags("Conveyor", "Laser Station")] IN2_0Left_1_lift_cylinder_Extend_InPos,
        [Description(""), EEGroup("IN2"), EENumber("IN2_1"), Tags("Conveyor", "Laser Station")] IN2_1Left_1_lift_cylinder_retract_InPos,
        [Description(""), EEGroup("IN2"), EENumber("IN2_2"), Tags("Conveyor", "Laser Station")] IN2_2Right_1_lift_cylinder_Extend_InPos,
        [Description(""), EEGroup("IN2"), EENumber("IN2_3"), Tags("Conveyor", "Laser Station")] IN2_3Right_1_lift_cylinder_retract_InPos,
        [Description(""), EEGroup("IN2"), EENumber("IN2_4"), Tags("Conveyor", "Foam Assembly Station")] IN2_4Left_2_lift_cylinder_Extend_InPos,
        [Description(""), EEGroup("IN2"), EENumber("IN2_5"), Tags("Conveyor", "Foam Assembly Station")] IN2_5Left_2_lift_cylinder_retract_InPos,
        [Description(""), EEGroup("IN2"), EENumber("IN2_6"), Tags("Conveyor", "Foam Assembly Station")] IN2_6Right_2_lift_cylinder_Extend_InPos,
        [Description(""), EEGroup("IN2"), EENumber("IN2_7"), Tags("Conveyor", "Foam Assembly Station")] IN2_7Right_2_lift_cylinder_retract_InPos,
        [Description(""), EEGroup("IN2"), EENumber("IN2_8"), Tags("Conveyor", "Recheck Station")] IN2_8Left_3_lift_cylinder_Extend_InPos,
        [Description(""), EEGroup("IN2"), EENumber("IN2_9"), Tags("Conveyor", "Recheck Station")] IN2_9Left_3_lift_cylinder_retract_InPos,
        [Description(""), EEGroup("IN2"), EENumber("IN2_10"), Tags("Conveyor", "Recheck Station")] IN2_10Right_3_lift_cylinder_Extend_InPos,
        [Description(""), EEGroup("IN2"), EENumber("IN2_11"), Tags("Conveyor", "Recheck Station")] IN2_11Right_3_lift_cylinder_retract_InPos,
        [Description(""), EEGroup("IN2"), EENumber("IN2_12"), Tags("Conveyor", "NG Station")] IN2_124_lift_cylinder_Extend_InPos,
        [Description(""), EEGroup("IN2"), EENumber("IN2_13"), Tags("Conveyor", "NG Station")] IN2_134_lift_cylinder_retract_InPos,
        [Description(""), EEGroup("IN2"), EENumber("IN2_14"), Tags("")] IN2_14backup,
        [Description(""), EEGroup("IN2"), EENumber("IN2_15"), Tags("")] IN2_15Reserve,


        [Description(""), EEGroup("IN3"), EENumber("IN3_0"), Tags("Conveyor", "Laser Station")] IN3_0Stopping_cylinder_1_extend_InPos,
        [Description(""), EEGroup("IN3"), EENumber("IN3_1"), Tags("Conveyor", "Laser Station")] IN3_1Stopping_cylinder_1_react_InPos,
        [Description(""), EEGroup("IN3"), EENumber("IN3_2"), Tags("Conveyor", "Foam Assembly Station")] IN3_2Stopping_cylinder_2_extend_InPos,
        [Description(""), EEGroup("IN3"), EENumber("IN3_3"), Tags("Conveyor", "Foam Assembly Station")] IN3_3Stopping_cylinder_2_react_InPos,
        [Description(""), EEGroup("IN3"), EENumber("IN3_4"), Tags("Conveyor", "Recheck Station")] IN3_4Stopping_cylinder_3_extend_InPos,
        [Description(""), EEGroup("IN3"), EENumber("IN3_5"), Tags("Conveyor", "Recheck Station")] IN3_5Stopping_cylinder_3_react_InPos,
        [Description(""), EEGroup("IN3"), EENumber("IN3_6"), Tags("Conveyor", "NG Station")] IN3_6Stopping_cylinder_4_extend_InPos,
        [Description(""), EEGroup("IN3"), EENumber("IN3_7"), Tags("Conveyor", "NG Station")] IN3_7Stopping_cylinder_4_react_InPos,
        [Description(""), EEGroup("IN3"), EENumber("IN3_8"), Tags("")] IN3_8Reserve,
        [Description(""), EEGroup("IN3"), EENumber("IN3_9"), Tags("Conveyor", "Recheck Station")] IN3_9Claw_extend_in_position,
        [Description(""), EEGroup("IN3"), EENumber("IN3_10"), Tags("Conveyor", "Recheck Station")] IN3_10Claw_retract_in_position,
        [Description(""), EEGroup("IN3"), EENumber("IN3_11"), Tags("")] IN3_11Reserve,//****
        [Description(""), EEGroup("IN3"), EENumber("IN3_12"), Tags("Picker", "Vacuum Air", "Foam Assembly Station")] IN3_12PNP_Gantry_vacuum1_Pressure_feedback,
        [Description(""), EEGroup("IN3"), EENumber("IN3_13"), Tags("Picker", "Vacuum Air", "Foam Assembly Station")] IN3_13PNP_Gantry_vacuum2_Pressure_feedback,
        [Description(""), EEGroup("IN3"), EENumber("IN3_14"), Tags("Picker", "Vacuum Air", "Foam Assembly Station")] IN3_14PNP_Gantry_vacuum3_Pressure_feedback,
        [Description(""), EEGroup("IN3"), EENumber("IN3_15"), Tags("Picker", "Vacuum Air", "Foam Assembly Station")] IN3_15PNP_Gantry_vacuum4_Pressure_feedback,

        [Description(""), EEGroup("IN4"), EENumber("IN4_0"), Tags("Feeder 1", "Laser Station")] IN4_0Initialized_feeder1,
        [Description(""), EEGroup("IN4"), EENumber("IN4_1"), Tags("Feeder 1", "Laser Station")] IN4_1Alarm_feeder1,
        [Description(""), EEGroup("IN4"), EENumber("IN4_2"), Tags("Feeder 1", "Laser Station")] IN4_2Platform_has_label_feeder1,
        [Description(""), EEGroup("IN4"), EENumber("IN4_3"), Tags("Feeder 1", "Laser Station")] IN4_3Backup_Platform_2_has_label_feeder1,
        [Description(""), EEGroup("IN4"), EENumber("IN4_4"), Tags("Feeder 2", "Laser Station")] IN4_4BInitialized_feeder2,
        [Description(""), EEGroup("IN4"), EENumber("IN4_5"), Tags("Feeder 2", "Laser Station")] IN4_51Alarm_feeder2,
        [Description(""), EEGroup("IN4"), EENumber("IN4_6"), Tags("Feeder 2", "Laser Station")] IN4_6Platform_has_label_feeder2,
        [Description(""), EEGroup("IN4"), EENumber("IN4_7"), Tags("Feeder 2", "Laser Station")] IN4_7Backup_Platform_2_has_label_feeder2,
        [Description(""), EEGroup("IN4"), EENumber("IN4_8"), Tags("Feeder 1", "Laser Station")] IN4_8Feeder1_limit_cylinder_extend_InPos,
        [Description(""), EEGroup("IN4"), EENumber("IN4_9"), Tags("Feeder 1", "Laser Station")] IN4_9Feeder1_limit_cylinder_retract_InPos,
        [Description(""), EEGroup("IN4"), EENumber("IN4_10"), Tags("Feeder 2", "Laser Station")] IN4_10Feeder2_limit_cylinder_extend_InPos,
        [Description(""), EEGroup("IN4"), EENumber("IN4_11"), Tags("Feeder 2", "Laser Station")] IN4_11Feeder2_limit_cylinder_retract_InPos,
        [Description(""), EEGroup("IN4"), EENumber("IN4_12"), Tags("Feeder 1", "Laser Station")] IN4_12Feeder1_drawer_InPos,
        [Description(""), EEGroup("IN4"), EENumber("IN4_13"), Tags("Feeder 2", "Laser Station")] IN4_13Feeder2_drawer_InPos,
        [Description(""), EEGroup("IN4"), EENumber("IN4_14"), Tags("")] IN4_14Reserve,
        [Description(""), EEGroup("IN4"), EENumber("IN4_15"), Tags("System", "Vacuum Air")] IN4_15Compressed_Air_Pressure, // Compressed Air Present

        [Description(""), EEGroup("IN5"), EENumber("IN5_0"), Tags("Feeder 1", "Vacuum Air", "Foam Assembly Station")] IN5_0Feeder_vacuum1_Pressure_feedback,
        [Description(""), EEGroup("IN5"), EENumber("IN5_1"), Tags("Feeder 1", "Vacuum Air", "Foam Assembly Station")] IN5_1Feeder_vacuum2_Pressure_feedback,
        [Description(""), EEGroup("IN5"), EENumber("IN5_2"), Tags("Feeder 2", "Vacuum Air", "Foam Assembly Station")] IN5_2Feeder_vacuum3_Pressure_feedback,
        [Description(""), EEGroup("IN5"), EENumber("IN5_3"), Tags("Feeder 2", "Vacuum Air", "Foam Assembly Station")] IN5_3Feeder_vacuum4_Pressure_feedback,
        [Description(""), EEGroup("IN5"), EENumber("IN5_4"), Tags("Door")] IN5_4Door_opened_lock1, // Only indicate door is closed, not locked
        [Description(""), EEGroup("IN5"), EENumber("IN5_5"), Tags("Door")] IN5_5Door_opened_lock2,
        [Description(""), EEGroup("IN5"), EENumber("IN5_6"), Tags("Door")] IN5_6Door_opened_lock3,
        [Description(""), EEGroup("IN5"), EENumber("IN5_7"), Tags("Door")] IN5_7Door_opened_lock4,
        [Description(""), EEGroup("IN5"), EENumber("IN5_8"), Tags("Button")] IN5_8Run,
        [Description(""), EEGroup("IN5"), EENumber("IN5_9"), Tags("Button")] IN5_9Stop,
        [Description(""), EEGroup("IN5"), EENumber("IN5_10"), Tags("Button")] IN5_10Feeder1, //physical button input
        [Description(""), EEGroup("IN5"), EENumber("IN5_11"), Tags("Button")] IN5_11Feeder2,
        [Description(""), EEGroup("IN5"), EENumber("IN5_12"), Tags("Button")] IN5_12Reset,
        [Description(""), EEGroup("IN5"), EENumber("IN5_13"), Tags("Button")] IN5_13emergency_stop,
        [Description(""), EEGroup("IN5"), EENumber("IN5_14"), Tags("System", "Safety Relay")] IN5_14SSR1_OK_emergency_stop,
        [Description(""), EEGroup("IN5"), EENumber("IN5_15"), Tags("System", "Safety Relay")] IN5_15SSR2_OK_LOCK,


        [Description(""), EEGroup("IN6"), EENumber("IN6_0"), Tags("Conveyor", "NG Station")] IN6_0NG_plate_1_in_position, // NG tray present
        [Description(""), EEGroup("IN6"), EENumber("IN6_1"), Tags("Conveyor", "Tray Type sensor")] IN6_1plate_type1, //Differentiate type Block high
        [Description(""), EEGroup("IN6"), EENumber("IN6_2"), Tags("Conveyor", "Tray Type sensor")] IN6_2plate_type2,
        [Description(""), EEGroup("IN6"), EENumber("IN6_3"), Tags("Conveyor", "Tray Type sensor")] IN6_3plate_type3,
        [Description(""), EEGroup("IN6"), EENumber("IN6_4"), Tags("Conveyor", "Tray Type sensor")] IN6_4plate_type4,
        [Description(""), EEGroup("IN6"), EENumber("IN6_5"), Tags("Conveyor", "Tray Type sensor")] IN6_5plate_type5,
        [Description(""), EEGroup("IN6"), EENumber("IN6_6"), Tags("Conveyor", "Recheck Station")] IN6_6plate_has_left_Behind_the_stopping_cylinder3,
        [Description(""), EEGroup("IN6"), EENumber("IN6_7"), Tags("Conveyor", "NG Station")] IN6_7plate_has_left_Behind_the_stopping_cylinder4,
        [Description(""), EEGroup("IN6"), EENumber("IN6_8"), Tags("")] IN6_8Reserve,
        [Description(""), EEGroup("IN6"), EENumber("IN6_9"), Tags("")] IN6_9Reserve,
        [Description(""), EEGroup("IN6"), EENumber("IN6_10"), Tags("")] IN6_10Reserve,
        [Description(""), EEGroup("IN6"), EENumber("IN6_11"), Tags("")] IN6_11Reserve,
        [Description(""), EEGroup("IN6"), EENumber("IN6_12"), Tags("")] IN6_12Reserve,
        [Description(""), EEGroup("IN6"), EENumber("IN6_13"), Tags("")] IN6_13Reserve,
        [Description(""), EEGroup("IN6"), EENumber("IN6_14"), Tags("")] IN6_14Reserve,
        [Description(""), EEGroup("IN6"), EENumber("IN6_15"), Tags("")] IN6_15Reserve,


        [Description(""), EEGroup("IN7"), EENumber("IN7_0"), Tags("Conveyor", "SMEMA")] IN7_0BOARD_AVAILABLE, // SMEMA
        [Description(""), EEGroup("IN7"), EENumber("IN7_1"), Tags("Conveyor", "SMEMA")] IN7_1FAILED_BOARD_AVAILABLE_OPTIONAL,
        [Description(""), EEGroup("IN7"), EENumber("IN7_2"), Tags("Conveyor", "SMEMA")] IN7_2MACHINE_READY_TO_RECEIVE,
        [Description(""), EEGroup("IN7"), EENumber("IN7_3"), Tags("")] IN7_3Reserve,
        [Description(""), EEGroup("IN7"), EENumber("IN7_4"), Tags("")] IN7_4Reserve,
        [Description(""), EEGroup("IN7"), EENumber("IN7_5"), Tags("")] IN7_5Reserve,
        [Description(""), EEGroup("IN7"), EENumber("IN7_6"), Tags("")] IN7_6Reserve,
        [Description(""), EEGroup("IN7"), EENumber("IN7_7"), Tags("")] IN7_7Reserve,
        [Description(""), EEGroup("IN7"), EENumber("IN7_8"), Tags("")] IN7_8Reserve,
        [Description(""), EEGroup("IN7"), EENumber("IN7_9"), Tags("")] IN7_9Reserve,
        [Description(""), EEGroup("IN7"), EENumber("IN7_10"), Tags("")] IN7_10Reserve,
        [Description(""), EEGroup("IN7"), EENumber("IN7_11"), Tags("")] IN7_11Reserve,
        [Description(""), EEGroup("IN7"), EENumber("IN7_12"), Tags("")] IN7_12Reserve,
        [Description(""), EEGroup("IN7"), EENumber("IN7_13"), Tags("")] IN7_13Reserve,
        [Description(""), EEGroup("IN7"), EENumber("IN7_14"), Tags("")] IN7_14Reserve,
        [Description(""), EEGroup("IN7"), EENumber("IN7_15"), Tags("")] IN7_15Reserve
    }


    class IOManager
    {
        private Dictionary<IO_OutFunction_Table, int> IO_OutFunctionnames = new Dictionary<IO_OutFunction_Table, int>();
        private Dictionary<IO_INFunction_Table, int> IO_INFunctionnames = new Dictionary<IO_INFunction_Table, int>();
        public int[] OutIO_status = new int[200];
        public int[] INIO_status = new int[200];

        private static IOManager _instance;
        private IOManager()
        {
            LoadIOPoint();
            for (int i = 0; i < 200; i++)
            {
                OutIO_status[i] = -1;
                INIO_status[i] = -1;
            }
        }
        private static readonly object _lock = new object();
        public static IOManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new IOManager();
                }
                return _instance;
            }
        }

        private void LoadIOPoint()
        {
            string filePath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "IOPoint.json");// 获取文件路径
            string json = File.ReadAllText(filePath);// 读取JSON文件并反序列化为对象
            JObject obj = JObject.Parse(json);
            foreach (string name in Enum.GetNames(typeof(IO_OutFunction_Table)))
            {
                if (Enum.TryParse<IO_OutFunction_Table>(name, out var func))
                {
                    short value = obj["Out"][name].Value<short>();
                    IO_OutFunctionnames.Add(func, value);
                }
                else
                {
                    Console.WriteLine("转换失败：不是有效的枚举名");
                }
            }

            foreach (string name in Enum.GetNames(typeof(IO_INFunction_Table)))
            {
                if (Enum.TryParse<IO_INFunction_Table>(name, out var func))
                {
                    short value = obj["IN"][name].Value<short>();
                    IO_INFunctionnames.Add(func, value);
                }
                else
                {
                    Console.WriteLine("转换失败：不是有效的枚举名");
                }
            }
        }
        private static readonly object _instanceLock = new object();
        private static readonly object _instanceLock2 = new object();
        //public void ReadIO_status()
        //{
        //    //循环读取输出IO
        //    Task.Run(new Action(() =>
        //    {
        //        Thread.CurrentThread.Name = "OutIO_statusThread";

        //        while (true)
        //        {

        //            Parallel.ForEach(IO_OutFunctionnames, IOname =>
        //            {
        //                var IOnamekey = IOname.Key;
        //                var IOnamevalue = IOname.Value;
        //                bool IOstatus = false;
        //                lock (_instanceLock2)
        //                {
        //                    bool ret = ModbusTCPWorker.GetInstance().Read_Coil(IOname.Value, ref IOstatus);
        //                    if (ret == false)
        //                    {
        //                        OutIO_status[(int)IOnamekey] = -1;
        //                        //Logger.WriteLog($"{IOnamekey.ToString()}-{ret.ToString()}:-1");
        //                    }
        //                    else
        //                    {
        //                        if (IOstatus)
        //                        {
        //                            OutIO_status[(int)IOnamekey] = 0;
        //                            //Logger.WriteLog($"{IOnamekey.ToString()}-{ret.ToString()}:0");
        //                        }
        //                        else
        //                        {
        //                            OutIO_status[(int)IOnamekey] = 1;
        //                            //Logger.WriteLog($"{IOnamekey.ToString()}-{ret.ToString()}:1");
        //                        }
        //                    }
        //                }
        //                Thread.Sleep(1000);
        //            });
        //            //Thread.Sleep(1);
        //        }
        //    }));
        //    //循环读取输入IO
        //    Task.Run(new Action(() =>
        //    {
        //        while (true)
        //        {
        //            Parallel.ForEach(IO_INFunctionnames, IOname =>
        //           {
        //               var IOnamekey = IOname.Key;
        //               var IOnamevalue = IOname.Value;
        //               bool IOstatus = false;
        //               lock (_instanceLock)
        //               {
        //                   bool ret = ModbusTCPWorker.GetInstance().Read_Coil(IOname.Value, ref IOstatus);
        //                   if (ret == false)
        //                   {
        //                       INIO_status[(int)IOnamekey] = -1;
        //                    }
        //                   else
        //                   {
        //                       if (IOstatus)
        //                       {
        //                           INIO_status[(int)IOnamekey] = 0;
        //                       }
        //                       else
        //                       {
        //                           INIO_status[(int)IOnamekey] = 1;
        //                        }

        //                   }
        //               }
        //               Thread.Sleep(5);


        //           });
        //            //Thread.Sleep(1000);
        //        }
        //    }));

        //}

        public void ReadIO_loop()
        {
            //循环读取输出IO
            Task.Run(new Action(() =>
            {
                Thread.CurrentThread.Name = "OutIO_statusThread";

                var first = (int)IO_INFunctionnames.First().Key;
                var length = (ushort)IO_OutFunctionnames.Count;
                bool[] IOstatusOut = new bool[length];
                //while (true)
                {
                    bool ret = ModbusTCPWorker.GetInstance().Read_Coil_Array(first, length, ref IOstatusOut);
                    int[] outputs = IOstatusOut.Select(x => x ? 0 : 1).ToArray();  //set true  to 0, false to 1
                    OutIO_status = ret ? outputs : Enumerable.Repeat(-1, length).ToArray();

                    Thread.Sleep(5);
                }
            }));
            //循环读取输入IO
            Task.Run(new Action(() =>
            {
                Thread.CurrentThread.Name = "INIO_statusThread";

                var first = (int)IO_INFunctionnames.First().Key;
                var length = (ushort)IO_INFunctionnames.Count;
                bool[] IOstatusIn = new bool[length];
                while (true)
                {
                    bool ret = ModbusTCPWorker.GetInstance().Read_Coil_Array(first, length, ref IOstatusIn);
                    int[] inputs = IOstatusIn.Select(x => x ? 0 : 1).ToArray();  //set true  to 0, false to 1
                    INIO_status = ret ? inputs : Enumerable.Repeat(-1, length).ToArray();

                    Thread.Sleep(1);
                }
            }));

        }
        //public bool ReadOutput(IO_OutFunction_Table output)
        //{
        //    bool IOstatusOut = false;
        //    bool ret = ModbusTCPWorker.GetInstance().Read_Coil((int)output, ref IOstatusOut);
        //    return ret? IOstatusOut : false;
        //}
        public bool GetOutputStatus(IO_OutFunction_Table output)
        {
            return OutIO_status[(int)output] == 0 ? true : false;
        }
        public bool IO_ControlStatus(IO_OutFunction_Table iO_OutFunction_Table, int writestatus)
        {

            if (writestatus == 1)
            {
                //if (!(OutIO_status[(int)iO_OutFunction_Table] == 0))//写IO状态为True
                //{
                //string err = string.Format("IO表里的值是true, 第{0}个线圈的值为true ", iO_OutFunction_Table.ToString(), writestatus.ToString());
                //Logger.WriteLog(err);
                bool Sucessstatus = ModbusTCPWorker.GetInstance().Write_Coil((int)iO_OutFunction_Table, true);
                if (!Sucessstatus)
                {
                    return false;
                }

                OutIO_status[(int)iO_OutFunction_Table] = 0;
                return true;
                //}
                return true;
            }

            if (writestatus == 0)
            {
                //if (OutIO_status[(int)iO_OutFunction_Table] == 0)//写IO状态为False
                //{
                //string err = string.Format("IO表里的值是false , 写第{0}个线圈的值为false ", iO_OutFunction_Table.ToString(), writestatus.ToString());
                //Logger.WriteLog(err);

                bool Sucessstatus = ModbusTCPWorker.GetInstance().Write_Coil((int)iO_OutFunction_Table, false);
                if (!Sucessstatus)
                {
                    return false;
                }
                OutIO_status[(int)iO_OutFunction_Table] = 1;
                return true;
                //}
                //return true;
            }
            return false;
        }
        public bool ReadIO(IO_INFunction_Table index)
        {
            if (INIO_status[(int)index] == 0)
            {
                return true;
            }
            else if (INIO_status[(int)index] == 1)
            {
                return false;
            }
            else
            {
                //ErrorManager.Current.Insert(ErrorCode.IOErr, $"Failed to read {index.ToString()}");
                return false;
            }
        }
        [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
        public class EEGroupAttribute : Attribute
        {
            public string GroupName { get; }
            public EEGroupAttribute(string name) => GroupName = name;
        }
        [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
        public class EENumberAttribute : Attribute
        {
            public string Name { get; }
            public EENumberAttribute(string name) => Name = name;
        }
        [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
        public class ModuleAttribute : Attribute
        {
            public string Name { get; }
            public ModuleAttribute(string name) => Name = name;
        }
        [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
        public class TagsAttribute : Attribute
        {
            public string[] Tags { get; }

            public TagsAttribute(params string[] tags)
            {
                Tags = tags;
            }
        }


        //public bool WriteIO_Truestatus(IO_OutFunction_Table iO_OutFunction_Table)//写IO状态为True
        //{
        //    if (!OutIO_status[(int)iO_OutFunction_Table])
        //    {
        //        bool Sucessstatus = ModbusTCPWorker.GetInstance().Write_Coil(IO_OutFunctionnames[iO_OutFunction_Table], true);
        //        if (!Sucessstatus)
        //        {
        //            return false;
        //        }

        //        OutIO_status[(int)iO_OutFunction_Table] = true;
        //        return true;
        //    }
        //    return true;
        //    //Console.WriteLine(  
        //}

        //public bool WriteIO_Falsestatus(IO_OutFunction_Table iO_OutFunction_Table)//写IO状态为False
        //{
        //    if (OutIO_status[(int)iO_OutFunction_Table])
        //    {
        //        bool Sucessstatus = ModbusTCPWorker.GetInstance().Write_Coil(IO_OutFunctionnames[iO_OutFunction_Table], false);
        //        if (!Sucessstatus)
        //        {
        //            return false;
        //        }
        //        OutIO_status[(int)iO_OutFunction_Table] = false;
        //        return true;
        //    }
        //    return true;
        //    //Console.WriteLine(  
        //}
    }
    public static class EnumMetadataHelper
    {
        public static string GetDescription(Enum value)
        {
            return value.GetAttribute<DescriptionAttribute>()?.Description ?? value.ToString();
        }

        public static string GetEEGroup(Enum value)
        {
            return value.GetAttribute<EEGroupAttribute>()?.GroupName ?? "Unknown";
        }
        public static string GetEENumber(Enum value)
        {
            return value.GetAttribute<EENumberAttribute>()?.Name ?? "Unknown";
        }
        public static string[] GetTags(Enum value)
        {
            return value.GetAttribute<TagsAttribute>()?.Tags ?? Array.Empty<string>();
        }

        // Generic attribute getter for enums
        public static T GetAttribute<T>(this Enum value) where T : Attribute
        {
            var fi = value.GetType().GetField(value.ToString());
            return fi?.GetCustomAttribute<T>();
        }

        public static List<EnumItem<TEnum>> ToEnumItemList<TEnum>() where TEnum : Enum
        {
            var list = new List<EnumItem<TEnum>>();

            foreach (TEnum enumVal in Enum.GetValues(typeof(TEnum)))
            {
                var description = enumVal.GetAttribute<DescriptionAttribute>()?.Description ?? enumVal.ToString();
                var group = enumVal.GetAttribute<EEGroupAttribute>()?.GroupName ?? "Unknown";
                var eeNum = enumVal.GetAttribute<EENumberAttribute>()?.Name ?? "Unknown";
                var tags = enumVal.GetAttribute<TagsAttribute>()?.Tags ?? Array.Empty<string>();

                list.Add(new EnumItem<TEnum>
                {
                    Value = enumVal,
                    //Description = description,
                    EENumber= eeNum,
                    EEGroup = group,
                    Tags = tags
                });
            }

            return list;
        }
    }
    public class EnumItem<TEnum> where TEnum : Enum
    {
        public TEnum Value { get; set; }
        public string Description => $"{Value.ToString().Replace(EENumber,"")}";
        public string EEGroup { get; set; }
        public string EENumber { get; set; }
        public string[] Tags { get; set; }
    }
}