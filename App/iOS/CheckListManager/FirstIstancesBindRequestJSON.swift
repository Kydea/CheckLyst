//
//  IstancesBindRequestJSON.swift
//  CheckListManager
//
//  Created by Simone Testa on 28/12/2017.
//  Copyright © 2017 Johnson&Johnson. All rights reserved.
//

//NOT USED IN THIS VERSION, MAYBE IN A FURTHER ONE TO SEND JSONS TO API
import Foundation

public struct FirstIstanceBindRequestJSON: Encodable
{
    public let data : dataFormatForFirstIstanceBindRequestJSON
}

public struct dataFormatForFirstIstanceBindRequestJSON : Encodable
{
    public let department : String?
    
    public let description : String?
    
    public let prototype : String?
    
    public let prototypeType : String?
    
    public let state : String?
    
    public let tagCode : String?
    
    public let tagValue : String?
    
    public let version : String?
}
