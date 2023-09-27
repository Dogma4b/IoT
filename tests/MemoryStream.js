
class MemoryStream {

    
    constructor(buffer){
        if(buffer)//read mode
        {
            this.buffer = buffer;
            this.position = 0;
            this.length = buffer.length;
        }
        else//write mode
        {
            this.buffer = Buffer.alloc(32);
            this.position = 0;
            this.length = 0; 
        }
    }
     
    //ensure that buffer.len >= position + len 
    _appendEnsure(len){
        var buflen = this.buffer.length;
        var thlen = this.length
        if(thlen+len>buflen){
            var newbuffer = Buffer.alloc(buflen*2);
            this.buffer.copy(newbuffer);
            this.buffer = newbuffer;  
        } 
    }
    // + shift stream length to max(length,position + len)  
    _appendCheck(len){
        this._appendEnsure(len);
        if(this.position+len>this.length)
        {
            this.length = this.position+len;
        }
    }


    writeByte(val){
        this._appendCheck(1);
        this.buffer.writeUInt8(val,this.position);
        this.position +=1; 
    }

    writeInt16(val){
        this._appendCheck(2);
        this.buffer.writeInt16LE(val,this.position);
        this.position +=2; 
    }
    writeInt32(val){
        this._appendCheck(4);
        this.buffer.writeInt32LE(val,this.position);
        this.position +=4; 
    }
    writeInt64(val){
        this._appendCheck(8);
        this.buffer.writeBigInt64LE(val,this.position);
        this.position +=8; 
    }

    writeUInt16(val){
        this._appendCheck(2);
        this.buffer.writeUInt16LE(val,this.position);
        this.position +=2; 
    }
    writeUInt32(val){
        this._appendCheck(4);
        this.buffer.writeUInt32LE(val,this.position);
        this.position +=4; 
    }
    writeUInt64(val){
        this._appendCheck(8);
        this.buffer.writeBigUInt64LE(val,this.position);
        this.position +=8; 
    }

    writeFloat(val){
        this._appendCheck(4);
        this.buffer.writeFloatLE(val,this.position);
        this.position +=4; 
    }
    writeDouble(val){
        this._appendCheck(8);
        this.buffer.writeDoubleLE(val,this.position);
        this.position +=8; 
    }

    writeRawString(val){
        var tempbuf = Buffer.alloc(val.length*2);
        var blen = tempbuf.write(val);
 
        this._appendCheck(blen);
        tempbuf.copy(this.buffer,this.position); 
        this.position +=blen; 
    }
    writeString(val)
    {
        var tempbuf = Buffer.alloc(val.length*2);
        var blen = tempbuf.write(val);

        this.writeUInt16(blen);

        this._appendCheck(blen);
        tempbuf.copy(this.buffer,this.position); 
        this.position +=blen; 
    }
    writeStream(val)
    {
        this._appendCheck(val.length);
        val.copy(this.buffer,this.position);
    }


    toBuffer(){
        return this.buffer.subarray(0,this.length);
    }
    copy(target,targetStart){
        this.buffer.copy(target,targetStart,0,this.length);
    }

    readByte(){
        var data = this.buffer.readUInt8(this.position);
        this.position += 1;
        return data;
    }

    readInt16(){
        var data = this.buffer.readInt16LE(this.position);
        this.position += 2;
        return data;
    }
    readInt32(){
        var data = this.buffer.readInt32LE(this.position);
        this.position += 4;
        return data;
    }
    readInt64(){
        var data = this.buffer.readBigInt64LE(this.position);
        this.position += 8;
        return data;
    }

    readUInt16(){
        var data = this.buffer.readUInt16LE(this.position);
        this.position += 2;
        return data;
    }
    readUInt32(){
        var data = this.buffer.readUInt32LE(this.position);
        this.position += 4;
        return data;
    }
    readUInt64(){
        var data = this.buffer.readBigUInt64LE(this.position);
        this.position += 8;
        return data;
    }
    
    readFloat(){
        var data = this.buffer.readFloatLE(this.position);
        this.position += 4;
        return data;
    }
    readDouble(){
        var data = this.buffer.readDoubleLE(this.position);
        this.position += 8;
        return data;
    }
    
    readRawString(blen){
        var data = this.buffer.toString('utf-8',this.position,this.position+blen)
        this.position += blen;
        return data;
    }
    readString(){
        var len = this.readUInt16();
        return this.readRawString(len);
    }
}

module.exports = MemoryStream

/*
//test

var teststr = new MemoryStream();
teststr.writeInt16(1123);
teststr.writeInt16(2324);
teststr.writeInt32(433333);
teststr.writeString("test string");
teststr.writeByte(2);
teststr.writeByte(1);
teststr.writeByte(2); 
teststr.writeInt32(-433333);

teststr.position = 0; 
console.log(teststr.readInt16());
console.log(teststr.readInt16());
console.log(teststr.readInt32());
console.log(teststr.readString());
console.log(teststr.readByte());
console.log(teststr.readByte());
console.log(teststr.readByte());
console.log(teststr.readInt32());

console.log(teststr.toBuffer());

*/