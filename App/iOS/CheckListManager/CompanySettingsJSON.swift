//
//  CompanySettingsJSON.swift
//  CheckListManager
//
//  Created by Simone Testa on 27/12/2017.
//  Copyright © 2017 Johnson&Johnson. All rights reserved.
//

import Foundation

public struct CompanySettingsJSON: Decodable
{
    
    public let requireUserReason : Bool
    
    public let webApplicationUrl : String
    
    public let inactivityTimeoutSeconds : Int
    
}

