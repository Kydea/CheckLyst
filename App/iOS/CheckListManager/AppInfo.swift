//
//  Version.swift
//  CheckListManager
//
//  Created by Lorenzo Spataro on 01/07/16.
//  Copyright © 2016 Johnson&Johnson. All rights reserved.
//


import Foundation


public struct AppInfo: Decodable
{
    
    var serverTime : String
    
    var version: Version
    
}

public struct Version : Decodable
{
    
    let version : String?
    
    let fileVersion : String?
    
    var buildDate : String?

}




