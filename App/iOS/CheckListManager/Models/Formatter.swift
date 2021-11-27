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
    
    fileprivate let testDateFormatter : DateFormatter
    
    fileprivate let testTimeFormatter : DateFormatter
    
    fileprivate let dateToTimeFormatter : DateFormatter
    
    fileprivate let stringToDateFormatter : DateFormatter
    
    fileprivate let infoTSFormatter : DateFormatter
    
    fileprivate let instanceDateTimeFormatter : DateFormatter
    
    
    
    fileprivate override init() {
        
        
        self.dateToDateTimeFormatter = DateFormatter()
        
        self.dateToTimeFormatter = DateFormatter()
        
        self.stringToDateFormatter = DateFormatter()
        
        self.infoTSFormatter = DateFormatter()
        
        self.instanceDateTimeFormatter = DateFormatter()
        
        self.testDateFormatter = DateFormatter()
        
        self.testTimeFormatter = DateFormatter()
        
        self.dateToDateTimeFormatter.dateFormat = "EEE, dd MMM yyyy HH:mm:ss zzz"
        self.dateToDateTimeFormatter.timeZone = TimeZone(abbreviation: "UTC")
        self.dateToTimeFormatter.dateFormat = "HH:mm"
        
        self.stringToDateFormatter.dateFormat = "yyyy-MM-dd'T'HH:mm:ss.SZ"
        self.stringToDateFormatter.timeZone = TimeZone(abbreviation: "UTC")
        
        self.infoTSFormatter.dateFormat = "yyyy-MM-dd HH:mm"
        
        self.instanceDateTimeFormatter.dateFormat = "yyyy-MM-dd HH:mm"
        
        self.testDateFormatter.dateFormat = "dd-MM-yyyy HH:mm:ss.SSS"
        
        self.testTimeFormatter.dateFormat = "HH:mm:ss"
        
    }
    
 
    
    func dateToDateTimeString(_ date: Date) -> String {
        if ((UserDefaults.standard.value(forKey: "timeZone") as! String) != self.dateToDateTimeFormatter.timeZone.identifier) {
            self.dateToDateTimeFormatter.timeZone = TimeZone(identifier: UserDefaults.standard.value(forKey: "timeZone") as! String)
        }
        return self.dateToDateTimeFormatter.string(from: date)
        
    }
    
    func dateFromTimeString(_ date: String) -> Date {
        return self.dateToDateTimeFormatter.date(from: date)!
    }
    
    func stringFromTestDate(_ date: Date, timeOnly: Bool = false) -> String {
        if (timeOnly) {
            return self.testTimeFormatter.string(from: date)
        }
        return self.testDateFormatter.string(from: date)
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
        if ((UserDefaults.standard.value(forKey: "timeZone") as! String) != self.infoTSFormatter.timeZone.identifier) {
            self.infoTSFormatter.timeZone = TimeZone(identifier: UserDefaults.standard.value(forKey: "timeZone") as! String)
        }
        return self.infoTSFormatter.string(from: date)
        
    }
    
    func stringFromServerToReadbleString(_ date:String) -> String {
        if ((UserDefaults.standard.value(forKey: "timeZone") as! String) != self.instanceDateTimeFormatter.timeZone.identifier) {
            self.instanceDateTimeFormatter.timeZone = TimeZone(identifier: UserDefaults.standard.value(forKey: "timeZone") as! String)
        }
        return self.instanceDateTimeFormatter.string(from: (self.stringToDate(date) as Date) )
    }
    
    
}
