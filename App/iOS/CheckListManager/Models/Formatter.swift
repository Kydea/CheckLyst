//
//  Formatter.swift
//  PowerVision
//
//  Created by Giacomo Tomolillo on 11/12/15.
//  Copyright © 2015 Matematici. All rights reserved.
//

import UIKit



class Formatter: NSObject {
    
    
    static let instance = Formatter()
    
    fileprivate let dateToDateTimeFormatter : DateFormatter
    
    fileprivate let dateToTimeFormatter : DateFormatter
    
    fileprivate let stringToDateFormatter : DateFormatter
    
    fileprivate let infoTSFormatter : DateFormatter
    
    
    
    fileprivate override init() {
        
        
        self.dateToDateTimeFormatter = DateFormatter()
        
        self.dateToTimeFormatter = DateFormatter()
        
        self.stringToDateFormatter = DateFormatter()
        
        self.infoTSFormatter = DateFormatter()
        
        self.dateToDateTimeFormatter.dateFormat = "EEE, dd MMM yyyy HH:mm:ss zzz"
        self.dateToDateTimeFormatter.locale = Locale(identifier: "en_US")
        
        self.dateToTimeFormatter.dateFormat = "HH:mm"
        
        self.stringToDateFormatter.dateFormat = "yyyy-MM-dd'T'HH:mm:ss.SZ"
        
        //self.stringToDateFormatter.timeZone = TimeZone(abbreviation: "UTC")
        self.infoTSFormatter.timeZone = TimeZone(abbreviation: "UTC")
        self.infoTSFormatter.dateFormat = "EEE, dd MMM yyyy HH:mm"
        
    }
    
 
    
    func dateToDateTimeString(_ date: Date) -> String {
        
        return self.dateToDateTimeFormatter.string(from: date)
        
    }
    
    func dateFromTimeString(_ date: String) -> Date {
        return self.dateToDateTimeFormatter.date(from: date)!
    }
    
    
    
    
    func dateToTimeString(_ date: Date) -> String {
        
        let string = self.dateToDateTimeFormatter.string(from: date)
        
        var array = string.components(separatedBy: " ")
        
        return array[1]
    }
    
    
    
    func stringToDate(_ dateString:String)-> Date {
        
        return stringToDateFormatter.date(from: dateString)!
        
    }
    
    func getInfoTSFromDate(_ date: Date) -> String {
        return self.infoTSFormatter.string(from: date)
    }
    
    func stringFromServerToReadbleString(_ str:String) -> String {
        let aux = str.split(separator: "T")
        let date = aux[0].replacingOccurrences(of: "-", with: "/")
        let time = aux[1].split(separator: ".")[0]
        let formattedStr = date + " " + time
        return formattedStr
    }
    
    
}
