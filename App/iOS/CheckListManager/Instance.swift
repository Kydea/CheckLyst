//
//  Instance.swift
//  CheckListManager
//
//  Created by Lorenzo Spataro on 21/06/16.
//  Copyright © 2016 Johnson&Johnson. All rights reserved.
//

import Foundation

public struct ListIstancesJSON: Decodable
    
{
    public let count : Int
    
    public let pagesCount : Int
    
    public let currentPage : Int
    
    public let itemsPerPage : Int
    
    public let data: [Instance?]
    
    public let messages : [String?]
}

public struct Instance: Decodable


{
    public let id : Int
    
    public let code : String
    
    public let year : String
    
    public let number : Int
    
    public let description: String?
    
    public let state : String
    
    public let departmentName : String
    
    public let prototypeTypeName : String
    
    public let prototypeName : String
    
    public let version : Int
    
    public let createdBy: String?
    
    public var createdOn: String?
    
    public let modifiedBy: String?
    
    public var modifiedOn: String?
    
    public var codeLabel: String { return prototypeName + "." + String(version) + "." + year + "." + String(number) }
    
}
